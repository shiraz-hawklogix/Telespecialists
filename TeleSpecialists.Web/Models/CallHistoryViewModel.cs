using System;

namespace TeleSpecialists.Models
{
    public class CallHistoryViewModel
    {
        public string callId { get; set; }
        public string agent { get; set; }
        public string agentName { get; set; }
        public string agentExtension { get; set; }
        public string callObjectId { get; set; }
        public string callResult { get; set; }
        public string callType { get; set; }
        public string formatAPICallType { get; set; }
        public string campaign { get; set; }
        public string comments { get; set; }
        public DateTime handleTime { get; set; }
        public string subject { get; set; }
        public string sessionId { get; set; }
        public DateTime wrapTime { get; set; }
        public int duration { get; set; }
        public int talkAndHoldDuration { get; set; }
        public string callbackId { get; set; }
        public string callbackNumber { get; set; }
    }
    public class InitializeCallHistoryViewModel
    {
        public string ANI { get; set; }
        public string DNIS { get; set; }
        public string TimeStamp { get; set; }
        public string CallId { get; set; }
        public string CampaignId { get; set; }
        public string Customer { get; set; }
    }
}