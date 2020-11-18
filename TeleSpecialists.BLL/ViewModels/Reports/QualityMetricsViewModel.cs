using System;
using System.Collections.Generic;
using TeleSpecialists.BLL.Helpers;

namespace TeleSpecialists.BLL.ViewModels.Reports
{
    public class QualityMetricsViewModel
    {
        public List<string> Physicians { get; set; }
        public List<Guid> Facilities { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<int> BillingCode { get; set; }
        public List<int> CaseType { get; set; }
        public List<int> CaseStatus { get; set; }
        public List<int> WorkFlowType { get; set; }
        public List<int> CallType { get; set; }
        public List<int> CallerSource { get; set; }
        public QualityMetricsAdvance AdvanceSearchCriteria { get; set; } = new QualityMetricsAdvance();
        public bool IncludeTime { get; set; }
        public string TimeFrame { get; set; }
        public string TimeCycle { get; set; }
        public List<string> QPSNumbers { get; set; }

        public List<bool> tPA { get; set; }
        public List<bool> eAlert { get; set; }
        public string DefaultType { get; set; }
        public string Blast { get; set; }
        public List<int?> states { get; set; }
        public List<Guid?> Specialist { get; set; }
        public List<string> Credentialing { get; set; }
        public bool WakeUpStroke { get; set; }
    }
    public class QualityMetricsAdvance
    {
        public BenchMarkCriteria HandleTime { get; set; }
        public BenchMarkCriteria AssignmentTime { get; set; }
        public BenchMarkCriteria BedsideResponseTime { get; set; }
        public BenchMarkCriteria LogInHandleTime { get; set; }
        public BenchMarkCriteria OnScreenTime { get; set; }
        public BenchMarkCriteria ActivationTime { get; set; }
        public BenchMarkCriteria ArrivalToNeedleTime { get; set; }
        public BenchMarkCriteria PhysicianMDM { get; set; }
        public BenchMarkCriteria TPAAdministratorTime { get; set; }
        public BenchMarkCriteria TSResponseTime { get; set; }
        public BenchMarkCriteria VerbalOrderToCPOEOrder { get; set; }
        public BenchMarkCriteria CPOEOrderToNeedle  { get; set; }
        public BenchMarkCriteria StartAcceptTime { get; set; }
        public BenchMarkCriteria SymptomsToNeedleTime { get; set; }
    }
    public class BenchMarkCriteria
    {
        public ComparisonOperator ComparisonOperator { get; set; }
        public TimeSpan TimeToEvaluate { get; set; }
    }
}
