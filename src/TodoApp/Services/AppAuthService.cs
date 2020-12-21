using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Stl.Async;
using Stl.Concurrency;
using Stl.Fusion;
using Stl.Fusion.Authentication;
using Stl.Fusion.Authentication.Internal;
using Stl.Time;
using TodoApp.Helpers;

namespace TodoApp.Services
{
    [ComputeService]
    public class AppAuthService : DbServiceBase<AppDbContext>, IServerSideAuthService
    {
        protected IMomentClock Clock { get; }

        public AppAuthService(IServiceProvider services, IMomentClock clock) : base(services)
            => Clock = clock;

        public async Task SignInAsync(User user, Session session, CancellationToken cancellationToken = default)
        {
            if (await IsSignOutForcedAsync(session, cancellationToken).ConfigureAwait(false))
                throw Errors.ForcedSignOut();

            await using var dbContext = CreateDbContext();
            await using var tx = await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            var dbUser = await GetOrCreateUserAsync(dbContext, user, cancellationToken).ConfigureAwait(false);
            var dbSession = await GetOrCreateSessionAsync(dbContext, session, cancellationToken).ConfigureAwait(false);
            if (dbSession.UserId != dbUser.Id)
                dbSession.UserId = dbUser.Id;
            await dbContext.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(default);

            Computed.Invalidate(() => {
                GetUserAsync(session, default).Ignore();
                PseudoGetUserSessionsAsync(user.Id).Ignore();
            });
        }

        public async Task SignOutAsync(bool force, Session session, CancellationToken cancellationToken = default)
        {
            await using var dbContext = CreateDbContext();
            await using var tx = await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            var dbSession = await GetOrCreateSessionAsync(dbContext, session, cancellationToken).ConfigureAwait(false);
            var userId = dbSession.UserId;
            dbSession.IsSignOutForced = force;
            dbSession.UserId = null;
            await dbContext.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(default);

            Computed.Invalidate(() => {
                GetUserAsync(session, default).Ignore();
                if (userId != null)
                    PseudoGetUserSessionsAsync(userId).Ignore();
                if (force)
                    IsSignOutForcedAsync(session, default).Ignore();
            });
        }

        public Task SaveSessionInfoAsync(SessionInfo sessionInfo, Session session, CancellationToken cancellationToken = default)
        {
            if (sessionInfo.Id != session.Id)
                throw new ArgumentOutOfRangeException(nameof(sessionInfo));
            var now = Clock.Now.ToDateTime();
            sessionInfo.LastSeenAt = now;
            SessionInfos.AddOrUpdate(session.Id, sessionInfo, (sessionId, oldSessionInfo) => {
                sessionInfo.CreatedAt = oldSessionInfo.CreatedAt;
                return sessionInfo;
            });
            Computed.Invalidate(() => GetSessionInfoAsync(session, default));
            return Task.CompletedTask;
        }

        public async Task UpdatePresenceAsync(Session session, CancellationToken cancellationToken = default)
        {
            var sessionInfo = await GetSessionInfoAsync(session, cancellationToken).ConfigureAwait(false);
            var now = Clock.Now.ToDateTime();
            var delta = now - sessionInfo.LastSeenAt;
            if (delta < TimeSpan.FromSeconds(10))
                return; // We don't want to update this too frequently
            sessionInfo.LastSeenAt = now;
            await SaveSessionInfoAsync(sessionInfo, session, cancellationToken).ConfigureAwait(false);
        }

        // Compute methods

        public virtual Task<bool> IsSignOutForcedAsync(Session session, CancellationToken cancellationToken = default)
            => Task.FromResult(ForcedSignOuts.ContainsKey(session.Id));

        public virtual async Task<User> GetUserAsync(Session session, CancellationToken cancellationToken = default)
        {
            if (await IsSignOutForcedAsync(session, cancellationToken).ConfigureAwait(false))
                return new User(session.Id);
            return Users.GetValueOrDefault(session.Id) ?? new User(session.Id);
        }

        public virtual Task<SessionInfo> GetSessionInfoAsync(
            Session session, CancellationToken cancellationToken = default)
        {
            var now = Clock.Now.ToDateTime();
            var sessionInfo = SessionInfos.GetValueOrDefault(session.Id)
                ?? new SessionInfo(session.Id) {
                    CreatedAt = now,
                    LastSeenAt = now,
                };
            return Task.FromResult(sessionInfo)!;
        }

        public virtual async Task<SessionInfo[]> GetUserSessions(
            Session session, CancellationToken cancellationToken = default)
        {
            var user = await GetUserAsync(session, cancellationToken).ConfigureAwait(false);
            if (!user.IsAuthenticated)
                return Array.Empty<SessionInfo>();

            await PseudoGetUserSessionsAsync(user.Id).ConfigureAwait(false);
            var sessionIds = UserSessions.GetValueOrDefault(user.Id) ?? ImmutableHashSet<string>.Empty;
            var result = new List<SessionInfo>();
            foreach (var sessionId in sessionIds) {
                var tmpSession = new Session(sessionId);
                var sessionInfo = await GetSessionInfoAsync(tmpSession, cancellationToken).ConfigureAwait(false);
                result.Add(sessionInfo);
            }
            return result.OrderByDescending(si => si.LastSeenAt).ToArray();
        }

        [ComputeMethod]
        protected virtual Task<Unit> PseudoGetUserSessionsAsync(string userId) => TaskEx.UnitTask;

        // Private methods

        private async Task<DbUser> GetOrCreateUserAsync(AppDbContext dbContext, User user, CancellationToken cancellationToken)
        {
            var dbUser = await dbContext.Users.FindAsync(user.Id).ConfigureAwait(false);
            if (dbUser == null) {
                dbUser = new DbUser() {
                    Id = user.Id,
                    Name = user.Name,
                };
                await dbContext.Users.AddAsync(dbUser, cancellationToken);
            }
            return dbUser;
        }

        private async Task<DbSession> GetOrCreateSessionAsync(AppDbContext dbContext, Session session, CancellationToken cancellationToken)
        {
            var dbSession = await dbContext.Sessions.FindAsync(session.Id).ConfigureAwait(false);
            if (dbSession == null) {
                dbSession = new DbSession() { Id = session.Id };
                await dbContext.Sessions.AddAsync(dbSession, cancellationToken);
            }
            return dbSession;
        }
    }
}
