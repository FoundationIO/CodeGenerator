/**
Copyright (c) 2016 Foundation.IO (https://github.com/foundationio). All rights reserved.

This work is licensed under the terms of the BSD license.
For a copy, see <https://opensource.org/licenses/BSD-3-Clause>.
**/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Framework.Data.Utils;
using Framework.Infrastructure.Models.Result;
using Framework.Infrastructure.Utils;
using LinqToDB;
using LogR.Api.All.Controllers;
using LogR.Api.All.Service;
using LogR.Common.Enums;
using LogR.Common.Models.DB;
using LogR.Common.Models.DBView;
using LogR.Common.Models.SearchCriteria;
using LogR.Repository.Common;

namespace LogR.Api.All.Repository.Wip
{
    public partial class WipRepository : BaseRepository
    {
        public IQueryable<EEntity123View> GetEEntity123sViewAsIQueryable()
        {
            return from item in dbMgr.GetTable<EEntity123>()
                       //%RepoJoin%
                   join t in dbMgr.GetTable<Tenant>() on a.TenantId equals t.TenantId
                   select new EEntity123View
                   {
                       TenantName = t.TenantName,
                       TenantId = t.TenantId,

                       //%RepoAssignment%
                   };
        }

        public async Task<DbReturnListModel<EEntity123View>> GetEEntity123sInternalAsync(EEntity123SearchCriteria search)
        {
            var sql1 = GetEEntity123sViewAsIQueryable();

            if (!search.Keyword.IsTrimmedStringNullOrEmpty())
            {
                sql1 = sql1.Where(x =>
                                x.EEntity123Id.ToString().Trim().ToUpper() == search.Keyword.Trim().ToUpper()
                                //%RepoWhereCheck%
                                );
            }

            //%RepoConditionalWhere%
            if (search.ShowDeleted == false)
                sql1 = sql1.Where(x => x.IsDeleted == false);

            return await sql1.ReturnListModelResultAsync(search);
        }

        public Task<DbReturnListModel<EEntity123View>> GetEEntity123sAsync(EEntity123SearchCriteria search)
        {
            return GetEEntity123sInternalAsync(search);
        }

        public async Task<EEntity123> GetEEntity123FromAnyByGuidAsync(Guid eentity123Guid)
        {
            return await dbMgr.FirstOrDefaultAsync<EEntity123>(x => x.EEntity123Guid == eentity123Guid);
        }

        public async Task<EEntity123View> GetEEntity123ViewFromAnyByGuidAsync(Guid eentity123Guid)
        {
            return await GetEEntity123sViewAsIQueryable().FirstOrDefaultAsync(x => x.EEntity123Guid == eentity123Guid);
        }

        public async Task<EEntity123> GetEEntity123FromAnyByGuidAndIdAsync(long eentity123Id, Guid eentity123Guid)
        {
            return await dbMgr.FirstOrDefaultAsync<EEntity123>(x => x.EEntity123Id == eentity123Id && x.EEntity123Guid == eentity123Guid);
        }

        public async Task<EEntity123View> GetEEntity123ViewFromAnyByGuidAndIdAsync(long eentity123Id, Guid eentity123Guid)
        {
            return await GetEEntity123sViewAsIQueryable().FirstOrDefaultAsync(x => x.EEntity123Id == eentity123Id && x.EEntity123Guid == eentity123Guid);
        }

        public async Task<EEntity123> GetEEntity123ByGuidAsync(Guid eentity123Guid)
        {
            return await dbMgr.FirstOrDefaultAsync<EEntity123>(x => x.EEntity123Guid == eentity123Guid && x.IsDeleted == false);
        }

        public Task<EEntity123View> GetEEntity123ViewByGuidAsync(Guid eentity123Guid)
        {
            var sql1 = GetEEntity123sViewAsIQueryable();

            sql1 = sql1.Where(x => x.EEntity123Guid == eentity123Guid && x.IsDeleted == false);

            return sql1.FirstOrDefaultAsync();
        }

        public async Task<EEntity123> GetEEntity123ByIdAsync(long eentity123Id)
        {
            return await dbMgr.FirstOrDefaultAsync<EEntity123>(x => x.EEntity123Id == eentity123Id && x.IsDeleted == false);
        }

        public Task<EEntity123View> GetEEntity123ViewByIdAsync(long eentity123Id)
        {
            var sql1 = GetEEntity123sViewAsIQueryable();

            sql1 = sql1.Where(x => x.EEntity123Id == eentity123Id && x.IsDeleted == false);
            return sql1.FirstOrDefaultAsync();
        }

        public Task<List<EEntity123>> GetAllEEntity123s()
        {
            return dbMgr.SelectAllAsync<EEntity123>();
        }

        public async Task<EEntity123> CreateEEntity123Async(EEntity123 eentity123, string createdBy)
        {
            eentity123.EEntity123Id = await dbMgr.InsertWithAuditAsync<EEntity123, long>(eentity123, createdBy);
            return eentity123;
        }

        public async Task<List<EEntity123>> CreateEEntity123sAsync(List<EEntity123> eentity123s, string createdBy)
        {
            var lst = await dbMgr.InsertWithAuditAsync<EEntity123, long>(eentity123s, createdBy);
            for (int i = 0; i < lst.Count; ++i)
                eentity123s[i].EEntity123Id = lst[i];
            return eentity123s;
        }

        public Task<bool> UpdateEEntity123Async(EEntity123 eentity123, string modifiedBy)
        {
            return dbMgr.UpdateWithAuditAsync<EEntity123>(eentity123, modifiedBy);
        }

        public async Task<bool> MarkEEntity123AsNotDeletedByGuidAsync(Guid eentity123Guid, string modifiedBy)
        {
            return await dbMgr.GetTable<EEntity123>().Where(x => x.EEntity123Guid == eentity123Guid).Set(x => x.IsDeleted, false).UpdateWithAuditAsync(modifiedBy) > 0;
        }

        public async Task<bool> MarkEEntity123AsNotDeletedByIdAsync(long eentity123Id, string modifiedBy)
        {
            return await dbMgr.GetTable<EEntity123>().Where(x => x.EEntity123Id == eentity123Id).Set(x => x.IsDeleted, false).UpdateWithAuditAsync(modifiedBy) > 0;
        }

        public async Task<bool> MarkEEntity123AsDeletedByGuidAsync(Guid eentity123Guid, string modifiedBy)
        {
            return await dbMgr.GetTable<EEntity123>().Where(x => x.EEntity123Guid == eentity123Guid).Set(x => x.IsDeleted, true).UpdateWithAuditAsync(modifiedBy) > 0;
        }

        public async Task<bool> MarkEEntity123AsDeletedByIdAsync(long eentity123Id, string modifiedBy)
        {
            return await dbMgr.GetTable<EEntity123>().Where(x => x.EEntity123Id == eentity123Id).Set(x => x.IsDeleted, true).UpdateWithAuditAsync(modifiedBy) > 0;
        }

        public async Task<bool> HardDeleteEEntity123ByGuidAsync(Guid eentity123Guid)
        {
            return await dbMgr.GetTable<EEntity123>().Where(x => x.EEntity123Guid == eentity123Guid).DeleteAsync() > 0;
        }

        public async Task<bool> HardDeleteEEntity123ByIdAsync(long eentity123Id)
        {
            return await dbMgr.GetTable<EEntity123>().Where(x => x.EEntity123Id == eentity123Id).DeleteAsync() > 0;
        }

        public Task<DbReturnListModel<ActivityView>> GetEEntity123ActivitiesAsync(long tenantId, long? tenantEntityId, ActivitySearchCriteria search)
        {
            var srch = LinqUtils.True<ActivityView>();
            srch = srch.And(x => x.TenantId == tenantId && x.ResourceTypeId == (int)ResourceTypes.EEntity123);
            if (tenantEntityId != null)
                srch = srch.And(x => x.TenantEntityId == tenantEntityId);

            return commonRepository.GetActivitiesByExpressionAsync(srch, search);
        }

        public Task<bool> IsEEntity123NameAlreadyExistsForOtherEEntity123Async(long eentity123Id, string eentity123Name, long tenantId, long? tenantEntityId)
        {
            var sql = GetEEntity123sViewAsIQueryable().Where(x => x.EEntity123Id != eentity123Id && x.EEntity123Name.ToLower().Trim() == eentity123Name.ToLower().Trim() && x.TenantId == tenantId);
            if (tenantEntityId != null)
                sql = sql.Where(x => x.TenantEntityId == tenantEntityId.Value);
            return sql.AnyAsync();
        }

        public Task<bool> IsEEntity123NameAlreadyExistsForOtherEEntity123Async(string eentity123Name, long tenantId, long? tenantEntityId)
        {
            var sql = GetEEntity123sViewAsIQueryable().Where(x => x.EEntity123Name.ToLower().Trim() == eentity123Name.ToLower().Trim() && x.TenantId == tenantId);
            if (tenantEntityId != null)
                sql = sql.Where(x => x.TenantEntityId == tenantEntityId.Value);
            return sql.AnyAsync();
        }
    }
}
