using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Models;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IDynamicGroupEvaluationService
    {
        Task<List<int>> EvaluateGroupRulesAsync(string userId, GroupRuleCriteria criteria);
        Task<bool> UpdateDynamicGroupMembershipsAsync(string userId, int groupId);
        Task UpdateAllDynamicGroupsAsync(string userId);
    }
}
