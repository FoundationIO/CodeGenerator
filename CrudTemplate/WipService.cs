/**
Copyright (c) 2016 Foundation.IO (https://github.com/foundationio). All rights reserved.

This work is licensed under the terms of the BSD license.
For a copy, see <https://opensource.org/licenses/BSD-3-Clause>.
**/
using Framework.Infrastructure.Exceptions;
using Framework.Infrastructure.Interfaces.DbAccess;
using Framework.Infrastructure.Interfaces.Helpers;
using Framework.Infrastructure.Logging;
using Framework.Infrastructure.Models.Result;
using Framework.Infrastructure.Utils;
using LogR.Api.All.Controllers;
using LogR.Api.All.Repository.Wip;
using LogR.Common.Enums;
using LogR.Common.Models.DB;
using LogR.Common.Models.DBView;
using LogR.Common.Models.ViewModels;
using LogR.Common.Models.SearchCriteria;
using LogR.Repository.Common;
using LogR.Service.App;
using LogR.Service.ObjectMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.AccessControl;

namespace LogR.Api.All.Service.Wip
{
    public partial class WipService : BaseAppService
    {
        public async Task<ReturnListModel<EEntity123Model>> GetEEntity123sAsync(EEntity123SearchCriteria search)
        {
            try
            {
                var (resultValue, canAssign, errorStr) = await permissionService.GetCanViewEEntity123PermissionParamsAsync();

                if (resultValue == false)
                {
                    return log.ErrorAndReturnListModel<EEntity123Model>(errorStr);
                }

                if (canAssign)
                {
                    search.TenantId = await currentSessionService.GetCurrentTenantIdAsNullableAsync();
                    search.TenantEntityId = await currentSessionService.GetCurrentTenantEntityIdAsNullableAsync();
                }

                return this.ConvertToList<EEntity123View, EEntity123Model>(await wipRepository.GetEEntity123sAsync(search));
            }
            catch (Exception ex)
            {
                return log.ErrorAndReturnListModel<EEntity123Model>($"Unexpected error when getting EEntity123s", ex);
            }
        }

        public async Task<ReturnModel<EEntity123Model>> GetEEntity123ByGuidAsync(Guid eentity123Guid)
        {
            try
            {
                var item = await wipRepository.GetEEntity123ViewByGuidAsync(eentity123Guid);
                if (item == null)
                {
                    return log.ErrorAndReturnModel<EEntity123Model>($"Unable to find the eentity123 for {eentity123Guid}");
                }

                var (resultValue, canAssign, errorStr) = await permissionService.GetCanViewEEntity123PermissionParamsAsync(item.TenantId, item.TenantEntityId);

                if (resultValue == false)
                {
                    return log.ErrorAndReturnModel<EEntity123Model>(errorStr);
                }

                return this.ConvertToItem<EEntity123View, EEntity123Model>(item);
            }
            catch (Exception ex)
            {
                return log.ErrorAndReturnModel<EEntity123Model>($"Unexpected error when getting eentity123 for - {eentity123Guid}", ex);
            }
        }

        public async Task<ReturnModel<EEntity123Model>> CreateEEntity123Async(EEntity123CreateModel eentity123)
        {
            try
            {
                if (eentity123 == null)
                {
                    return log.ErrorAndReturnModel<EEntity123Model>($"Unable to create EEntity123", new ReturnErrorItem("", "EEntity123 information is not valid"));
                }

                var (resultValue, canAssign, errorStr) = await permissionService.GetCanManageEEntity123PermissionParamsAsync(eentity123.TenantId, eentity123.TenantEntityId);

                if (resultValue == false)
                {
                    return log.ErrorAndReturnModel<EEntity123Model>(errorStr);
                }

                if (canAssign)
                {
                    eentity123.TenantId = await currentSessionService.GetCurrentTenantIdAsync();
                    //eentity123.TenantEntityId = await currentSessionService.GetCurrentTenantEntityIdAsNullableAsync();
                }

                var errorList = await IsEEntity123ModelValidForCreateAsync(eentity123);
                if (errorList != null)
                {
                    return log.ErrorAndReturnModel<EEntity123Model>($"Unable to create eentity123", errorList);
                }

                eentity123.EEntity123Guid = Guid.NewGuid();
                eentity123.IsDeleted = false;

                EEntity123 item;
                using (var trx = await transactionManager.BeginTransactionAsync())
                {
                    item = await wipRepository.CreateEEntity123Async(eentity123, currentUser.GetCurrentUserName());
                    if (item == null)
                    {
                        throw new Exception($"Unable to create eentity123");
                    }

                    await trx.CompleteAsync();
                }


                await auditService.EEntity123CreatedAsync(item.EEntity123Id);

                var view = await wipRepository.GetEEntity123ViewByIdAsync(item.EEntity123Id);

                if (view == null)
                {
                    return log.ErrorAndReturnModel<EEntity123Model>($"Unable to find the eentity123 for {item.EEntity123Guid}");
                }

                return this.ConvertToItem<EEntity123View, EEntity123Model>(view);
            }
            catch (RollbackException ex)
            {
                return log.ErrorAndReturnModel<EEntity123Model>($"Rollbacking the transaction when creating EEntity123 {eentity123?.EEntity123Guid}", ex);
            }
            catch (Exception ex)
            {
                return log.ErrorAndReturnModel<EEntity123Model>($"Unexpected error when creating EEntity123 {eentity123?.EEntity123Guid}", ex);
            }
        }

        public async Task<ReturnModel<EEntity123Model>> UpdateEEntity123Async(EEntity123UpdateModel eentity123)
        {
            try
            {
                if (eentity123 == null)
                {
                    return log.ErrorAndReturnModel<EEntity123Model>($"Unable to update EEntity123", new ReturnErrorItem("", "EEntity123 information is not valid"));
                }

                var existingEEntity123 = await wipRepository.GetEEntity123ViewFromAnyByGuidAndIdAsync(eentity123.EEntity123Id, eentity123.EEntity123Guid);
                if (existingEEntity123 == null)
                {
                    return log.ErrorAndReturnModel<EEntity123Model>($"Unable to find the eentity123 for {eentity123.EEntity123Id} and {eentity123.EEntity123Guid}");
                }

                var (resultValue, canAssign, errorStr) = await permissionService.GetCanManageEEntity123PermissionParamsAsync(existingEEntity123.TenantId, existingEEntity123.TenantEntityId);

                if (resultValue == false)
                {
                    return log.ErrorAndReturnModel<EEntity123Model>(errorStr);
                }

                if (canAssign)
                {
                    eentity123.TenantId = await currentSessionService.GetCurrentTenantIdAsync();
                    //eentity123.TenantEntityId = await currentSessionService.GetCurrentTenantEntityIdAsNullableAsync();
                }

                eentity123.CreatedBy = existingEEntity123.CreatedBy;
                eentity123.CreatedDate = existingEEntity123.CreatedDate;
                eentity123.IsDeleted = existingEEntity123.IsDeleted;

                var errorList = await IsEEntity123ModelValidForUpdateAsync(eentity123);
                if (errorList != null)
                {
                    return log.ErrorAndReturnModel<EEntity123Model>($"Unable to update eentity123", errorList);
                }

                bool result;
                using (var trx = await transactionManager.BeginTransactionAsync())
                {
                    result = await wipRepository.UpdateEEntity123Async(eentity123, currentUser.GetCurrentUserName());
                    if (result == false)
                    {
                        throw new RollbackException($"Unable to update eentity123");
                    }

                    await trx.CompleteAsync();
                }

                await auditService.EEntity123UpdatedAsync(eentity123.EEntity123Id);

                var view = await wipRepository.GetEEntity123ViewByIdAsync(eentity123.EEntity123Id);

                if (view == null)
                {
                    return log.ErrorAndReturnModel<EEntity123Model>($"Unable to find the eentity123 for {eentity123.EEntity123Guid}");
                }

                return this.ConvertToItem<EEntity123View, EEntity123Model>(view);
            }
            catch (Exception ex)
            {
                return log.ErrorAndReturnModel<EEntity123Model>($"Unexpected error when updating EEntity123 {eentity123?.EEntity123Guid}", ex);
            }
        }

        public async Task<ReturnModel<bool>> MarkEEntity123AsNotDeletedByGuidAsync(Guid eentity123Guid)
        {
            try
            {
                var eentity123 = await wipRepository.GetEEntity123ViewFromAnyByGuidAsync(eentity123Guid);
                if (eentity123 == null)
                {
                    return log.ErrorAndReturnModel<bool>($"Unable to find the eentity123 for {eentity123Guid}");
                }

                var (resultValue, canAssign, errorStr) = await permissionService.GetCanManageEEntity123PermissionParamsAsync(eentity123.TenantId, eentity123.TenantEntityId);

                if (resultValue == false)
                {
                    return log.ErrorAndReturnModel<bool>(errorStr);
                }

                bool result;
                using (var trx = await transactionManager.BeginTransactionAsync())
                {
                    result = await wipRepository.MarkEEntity123AsNotDeletedByGuidAsync(eentity123Guid, currentUser.GetCurrentUserName());
                    await trx.CompleteAsync();
                }

                await auditService.EEntity123DeletedAsync(eentity123.EEntity123Id);

                return new ReturnModel<bool>(result);
            }
            catch (Exception ex)
            {
                return log.ErrorAndReturnModel<bool>($"Unexpected error when marking eentity123 as not deleted {eentity123Guid}", ex);
            }
        }

        public async Task<ReturnModel<bool>> MarkEEntity123AsDeletedByGuidAsync(Guid eentity123Guid)
        {
            try
            {
                var eentity123 = await wipRepository.GetEEntity123ViewFromAnyByGuidAsync(eentity123Guid);
                if (eentity123 == null)
                {
                    return log.ErrorAndReturnModel<bool>($"Unable to find the eentity123 for {eentity123Guid}");
                }

                var (resultValue, canAssign, errorStr) = await permissionService.GetCanManageEEntity123PermissionParamsAsync(eentity123.TenantId, eentity123.TenantEntityId);

                if (resultValue == false)
                {
                    return log.ErrorAndReturnModel<bool>(errorStr);
                }

                bool result;
                using (var trx = await transactionManager.BeginTransactionAsync())
                {
                    result = await wipRepository.MarkEEntity123AsDeletedByGuidAsync(eentity123Guid, currentUser.GetCurrentUserName());
                    await trx.CompleteAsync();
                }

                await auditService.EEntity123DeletedAsync(eentity123.EEntity123Id);

                return new ReturnModel<bool>(result);
            }
            catch (Exception ex)
            {
                return log.ErrorAndReturnModel<bool>($"Unexpected error when marking eentity123 as deleted {eentity123Guid}", ex);
            }
        }

        public async Task<ReturnModel<bool>> HardDeleteEEntity123ByAsync(Guid eentity123Guid)
        {
            try
            {
                var eentity123 = await wipRepository.GetEEntity123ViewFromAnyByGuidAsync(eentity123Guid);
                if (eentity123 == null)
                {
                    return log.ErrorAndReturnModel<bool>($"Unable to find the eentity123 for {eentity123Guid}");
                }

                var (resultValue, canAssign, errorStr) = await permissionService.GetCanManageEEntity123PermissionParamsAsync(eentity123.TenantId, eentity123.TenantEntityId);

                if (resultValue == false)
                {
                    return log.ErrorAndReturnModel<bool>(errorStr);
                }


                bool result;

                using (var trx = await transactionManager.BeginTransactionAsync())
                {
                    result = await wipRepository.HardDeleteEEntity123ByGuidAsync(eentity123Guid);
                    await trx.CompleteAsync();
                }


                await auditService.HardDeleteEEntity123Async(eentity123.EEntity123Id);

                return new ReturnModel<bool>(result);
            }
            catch (Exception ex)
            {
                return log.ErrorAndReturnModel<bool>($"Unexpected error when deleting eentity123 {eentity123Guid}", ex);
            }
        }

        public async Task<List<ReturnErrorItem>> IsEEntity123ModelValidForCreateAsync(EEntity123Model eentity123Model)
        {
            return await IsEEntity123ModelValidForAllAsync(true, eentity123Model);
        }

        public async Task<List<ReturnErrorItem>> IsEEntity123ModelValidForUpdateAsync(EEntity123Model eentity123Model)
        {
            return await IsEEntity123ModelValidForAllAsync(false, eentity123Model);
        }

        private async Task<List<ReturnErrorItem>> IsEEntity123ModelValidForAllAsync(bool isCreate, EEntity123Model eentity123Model)
        {
            List<ReturnErrorItem> result = new List<ReturnErrorItem>();

            if (isCreate)
            {
                if (await wipRepository.IsEEntity123NameAlreadyExistsForOtherEEntity123Async(eentity123Model.EEntity123Name, eentity123Model.TenantId, eentity123Model.TenantEntityId))
                {
                    result.Add(new ReturnErrorItem("", "Another EEntity123 already exists"));
                }

                if (eentity123Model.EEntity123Id > 0)
                {
                    result.Add(new ReturnErrorItem("", "EEntity123 Identifier is not valid"));
                }
            }
            else
            {
                if (await wipRepository.IsEEntity123NameAlreadyExistsForOtherEEntity123Async(eentity123Model.EEntity123Id, eentity123Model.EEntity123Name, eentity123Model.TenantId, eentity123Model.TenantEntityId))
                {
                    result.Add(new ReturnErrorItem("", "Another EEntity123 already exists"));
                }

                if (eentity123Model.EEntity123Id <= 0)
                {
                    result.Add(new ReturnErrorItem("", "EEntity123 Unique Identifier is not valid"));
                }

                if (eentity123Model.EEntity123Guid == Guid.Empty)
                {
                    result.Add(new ReturnErrorItem("", "EEntity123 Identifier is not valid"));
                }

            }

            //%ServiceChecks%

            return result.Count > 0 ? result : null;
        }
    }
}
