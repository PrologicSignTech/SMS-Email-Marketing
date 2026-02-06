using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Message;
using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.Interfaces
{
    public interface IMessageService
    {
        Task<MessageDto?> GetMessageByIdAsync(string userId, int messageId);
        Task<PaginatedResult<MessageDto>> GetMessagesAsync(string userId, PagedRequest request);
        Task<PaginatedResult<MessageDto>> GetMessagesByCampaignAsync(string userId, int campaignId, PagedRequest request);
        Task<List<MessageDto>> GetMessagesByStatusAsync(string userId, MessageStatus status);
        Task<MessageDto> CreateMessageAsync(string userId, CreateMessageDto dto);
        Task<List<MessageDto>> CreateBulkMessagesAsync(string userId, BulkMessageRequestDto dto);
        Task<bool> UpdateMessageStatusAsync(string userId, int messageId, MessageStatusUpdateDto dto);
        Task<bool> RetryFailedMessageAsync(string userId, int messageId);
        Task<int> RetryFailedMessagesForCampaignAsync(string userId, int campaignId);
        Task<bool> CancelScheduledMessageAsync(string userId, int messageId);
        Task<MessageDeliveryReportDto> GetDeliveryReportAsync(string userId, int campaignId);
        Task ProcessMessageQueueAsync();
        Task<bool> SendMessageNowAsync(string userId, int messageId);
        Task<MessagePreviewDto> PreviewMessageAsync(string userId, MessagePreviewRequestDto request);
        Task<TestSendResultDto> SendTestMessageAsync(string userId, TestSendRequestDto request);
    }
}
