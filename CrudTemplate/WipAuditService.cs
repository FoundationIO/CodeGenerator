/**
Copyright (c) 2016 Foundation.IO (https://github.com/foundationio). All rights reserved.

This work is licensed under the terms of the BSD license.
For a copy, see <https://opensource.org/licenses/BSD-3-Clause>.
**/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.AccessControl;
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

namespace LogR.Service.App
{
    public partial class AuditService
    {
        public async Task EEntity123CreatedAsync(long eentity123Id)
        {
            await auditRepository.InsertAuditAsync(AuditOperationTypes.ResourceCreated, resourceTypeId: ResourceTypes.EEntity123, resourceName: "EEntity123", resourceId: eentity123Id, userId: await currentSessionService.GetCurrentUserIdAsNullableAsync(), userName: currentUserName, ipAddress: currentUserAddress, tenantEntityId: await currentSessionService.GetCurrentTenantEntityIdAsNullableAsync(), tenantEntityName: await currentSessionService.GetCurrentTenantEntityNameAsync(), tenantId: await currentSessionService.GetCurrentTenantIdAsNullableAsync(), tenantName: await currentSessionService.GetCurrentTenantNameAsync());
        }

        public async Task EEntity123UpdatedAsync(long eentity123Id)
        {
            await auditRepository.InsertAuditAsync(AuditOperationTypes.ResourceUpdated, resourceTypeId: ResourceTypes.EEntity123, resourceName: "EEntity123", resourceId: eentity123Id, userId: await currentSessionService.GetCurrentUserIdAsNullableAsync(), userName: currentUserName, ipAddress: currentUserAddress, tenantEntityId: await currentSessionService.GetCurrentTenantEntityIdAsNullableAsync(), tenantEntityName: await currentSessionService.GetCurrentTenantEntityNameAsync(), tenantId: await currentSessionService.GetCurrentTenantIdAsNullableAsync(), tenantName: await currentSessionService.GetCurrentTenantNameAsync());
        }

        public async Task EEntity123DeletedAsync(long eentity123Id)
        {
            await auditRepository.InsertAuditAsync(AuditOperationTypes.ResourceDeleted, resourceTypeId: ResourceTypes.EEntity123, resourceName: "EEntity123", resourceId: eentity123Id, userId: await currentSessionService.GetCurrentUserIdAsNullableAsync(), userName: currentUserName, ipAddress: currentUserAddress, tenantEntityId: await currentSessionService.GetCurrentTenantEntityIdAsNullableAsync(), tenantEntityName: await currentSessionService.GetCurrentTenantEntityNameAsync(), tenantId: await currentSessionService.GetCurrentTenantIdAsNullableAsync(), tenantName: await currentSessionService.GetCurrentTenantNameAsync());
        }

        public async Task EEntity123UnDeletedAsync(long eentity123Id)
        {
            await auditRepository.InsertAuditAsync(AuditOperationTypes.ResourceUndeleted, resourceTypeId: ResourceTypes.EEntity123, resourceName: "EEntity123", resourceId: eentity123Id, userId: await currentSessionService.GetCurrentUserIdAsNullableAsync(), userName: currentUserName, ipAddress: currentUserAddress, tenantEntityId: await currentSessionService.GetCurrentTenantEntityIdAsNullableAsync(), tenantEntityName: await currentSessionService.GetCurrentTenantEntityNameAsync(), tenantId: await currentSessionService.GetCurrentTenantIdAsNullableAsync(), tenantName: await currentSessionService.GetCurrentTenantNameAsync());
        }

        public async Task HardDeleteEEntity123Async(long eentity123Id)
        {
            await auditRepository.InsertAuditAsync(AuditOperationTypes.ResourceHardDeleted, resourceTypeId: ResourceTypes.EEntity123, resourceName: "EEntity123", resourceId: eentity123Id, userId: await currentSessionService.GetCurrentUserIdAsNullableAsync(), userName: currentUserName, ipAddress: currentUserAddress, tenantEntityId: await currentSessionService.GetCurrentTenantEntityIdAsNullableAsync(), tenantEntityName: await currentSessionService.GetCurrentTenantEntityNameAsync(), tenantId: await currentSessionService.GetCurrentTenantIdAsNullableAsync(), tenantName: await currentSessionService.GetCurrentTenantNameAsync());
        }
    }
}
