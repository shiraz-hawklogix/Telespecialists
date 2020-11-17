using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TeleSpecialists.BLL.Helpers
{
    //Enum Collections which are also present in database should reflect same values/Ids as the database

    public enum AuditRecordLogStatus
    {
        [Description("Login Success")]
        LoginSucess,
        [Description("Login Failed")]
        LoginFailed,
        [Description("Account Locked Out")]
        AccountLockOut,
        [Description("Require Varification")]
        RequireVarification,
        [Description("Access Denied")]
        AccessDenied,
        [Description("Login Attmpts Exceeded")]
        LoginAttemptsExceeded,
        [Description("Log Out")]
        LogOut,
        [Description("Remote Login")]
        RemoteLogin,
        [Description("Remote Log Out")]
        RemoteLogOut,
        [Description("User Created")]
        UserCreated,
        [Description("User Updated")]
        UserUpdated,
        [Description("Password Change")]
        PasswordChange,
        [Description("Password Added")]
        SetPassword,
        [Description("Login Removed")]
        RemoveLogin,
        [Description("Two Factor Enabled")]
        TwoFactorEnabled,
        [Description("Two Factor Disabled")]
        TwoFactorDisabled,
        [Description("Phone Number varified")]
        PhoneVrified,
        [Description("Phone Removed")]
        PhoneRemoved,
        [Description("External Login Success")]
        ExternalLoginSucess,
        [Description("External Login Failed")]
        ExternalLoginFailed,
        [Description("External Account Locked Out")]
        ExternalAccountLockOut,
        [Description("External Require Varification")]
        ExternalRequireVarification, 
        [Description("External Log Out")]
        ExternalLogOut,
        [Description("External Account Created")]
        ExternalAccountCreated,
       
    }

    public enum RequestStatus
    {

        Complete = 1,
        Pending = 2,
        InProcess = 3,
        Closed = 4,
        Canceled = 5,
        Completed = 6,
        Revised = 7,
        Recalled = 8
    }

    public enum UclTypes
    {
        
        BillingCode = 10,
        CaseType,
        CaseStatus,
        EMR,
        ContractType,
        CoverageType,
        FacilityType,

        LicenseType,
        LoginDelay,
        NoteType,
        ServiceType,
        State,
        StrokeDesignation,
        [Description("Alteplase/Activase Delay")]
        TpaDelay,
        [Description("Alteplase/Activase Candidate Types")]
        NonTPACandidate,
        [Description("Identification Type")]
        IdentificationType,
        StrokeTemplateRecommendations,
        [Description("Antiplatelet Therapy Recommended No Alteplase/Activase")]
        AntiplateletTherapyRecommendedNoTpa,
        [Description("Imaging Studies Recommended No Alteplase/Activase")]
        ImagingStudiesRecommendedNoTpa,
        [Description("Therapies No Alteplase/Activase")]
        TherapiesNoTpa,
        [Description("Dysphaghia Screen No Alteplase/Activase")]
        DysphaghiaScreenNoTpa,
        [Description("DVT Prophylaxis No Alteplase/Activase")]
        DVTProphylaxisNoTpa,
        [Description("Disposition No Alteplase/Activase")]
        DispositionNoTpa,
        [Description("Sign Out No Alteplase/Activase")]
        SignOutNoTpa,
        [Description("Contact Roles")]
        ContactRole,
        [Description("Caller Source")]
        CallerSource,
        [Description("Cart Location")]
        CartLocation,
        PacCaseType ,
        PacStatus ,
        [Description("State Consult Template")]
        StateConsultTemplate = 39,
        [Description("Imaging Studies Stat Consult")]
        ImagingStudiesStatConsult = 28,
       
        [Description("Therapies State")]
        TherapiesState = 29,
        
       // PacStatus = 35,
       // PacCaseType = 34,
        BedSize = 41,
        System = 42,
        Regional = 43,
        [Description("Sign Out Stat")]
        SignOutStat = 33,
        [Description("Other Work Up")]
        OtherWorkUp = 40,
        [Description("Past Medical History ")]
        PMH = 44,
        SleepCodes = 45,
        FacilityAI = 46


    }

    public enum PhysicianCaseAssignQueue
    {

        Accepted,
        Rejected,
        InQueue,
        Expired,
        WaitingForAction,
        NotAcceptedByAll,
        ManuallyAssigned
    }

    public enum MetricResponseStatus
    {
        [Description("Yes")]
        Yes = 1,
        [Description("No")]
        No = 2,
        [Description("N/A")]
        NA = 3
    }

    public enum Duration
    {

        Month,
        Year
    }

    public enum YesNoFilter
    {
        All,
        Yes,
        No
    }

    public enum ShiftType
    {
        All = 0,
        OnShift = 1,
        OffShift = 2
    }

    public enum PageSource
    {
        CaseListing = 1,
        Dashboard = 2,
        SignOutListing =3,
    }


    public enum StatusOptions
    {
        [Description("Yes")]
        Yes = 1,
        [Description("No")]
        No = 2
    }

    public enum LB2S2CriteriaOptions
    {
        [Description("Yes")]
        Yes = 1,
        [Description("No")]
        No = 0,
        [Description("UNK")]
        UNK = 2
    }

    public enum PatientType
    {
        [Description("EMS")]
        EMS = 1,

        //[Description("Triage")]
        [Description("Triage/Walk-In")]
        Triage = 3,

        [Description("Inpatient")]
        Inpatient = 2
    }


    public enum WeightUnit
    {
        [Description("kg")]
        kg,
        [Description("lbs")]
        lbs
    }

    public enum UserRoles
    {
        [Description("Physician")]
        Physician,
        [Description("Administrator")]
        Administrator,
        [Description("Navigator")]
        Navigator,
        [Description("Super Admin")]
        SuperAdmin,
        [Description("TeleCARE API")]
        TeleCareApi,
        [Description("Quality Team")]
        QualityTeam,
        [Description("Outsourced Agent")]
        OutSourcedAgent,
        [Description("Partner Physician")]
        PartnerPhysician,
        [Description("Facility Navigator")]
        FacilityNavigator,
        [Description("Outsourced Navigator")]
        OutsourcedNavigator,
        [Description("Facility Admin")]
        FacilityAdmin,
        [Description("Regional Medical Director")]
        RegionalMedicalDirector,
        [Description("Facility Physician")]
        FacilityPhysician,
        [Description("PAC Navigator")]
        PACNavigator,
        [Description("AOC")]
        AOC,
        [Description("Capacity Researcher")]
        CapacityResearcher,
        [Description("Credentialing Team")]
        CredentialingTeam,
        [Description("QPS")]
        QPS,
        [Description("RRC Manager")]
        RRCManager,
        [Description("RRC Director")]
        RRCDirector,
        [Description("VP Quality")]
        VPQuality,
        [Description("Finance")]
        Finance,
        [Description("Partner")]
        Partner,
        [Description("Quality Director")]
        QualityDirector,
        
        [Description("Medical Staff")]
        MedicalStaff

    }

    public enum PhysicianStatus
    {
        [Description("Available")]
        Available = 1,
        [Description("tPA")]
        TPA = 2,
        [Description("Stroke")]
        Stroke = 3,
        [Description("Rounding")]
        Rounding = 4,
        [Description("Not Available")]
        NotAvailable = 5,
        [Description("EEG")]
        EEG = 6,
        [Description("Phone Consult")]
        PhoneConsult = 7,
        [Description("STAT Consult")]
        STATConsult = 8

    }

    public enum ContractTypes
    {
        [Description("TeleStroke")]
        TeleStroke,
        [Description("TeleNeuro")]
        TeleNeuro
    }

    public enum CallType
    {
        Direct = 1, 
        Indirect = 2     
    }

    public enum CaseType
    {
        [Description("Stroke Alert")]
        StrokeAlert = 9,
        [Description("STAT Consult")]
        StatConsult = 10,
        [Description("Dr to Dr")]
        DrtoDr = 11,
        [Description("Nurse to Dr")]
        NursetoDr = 12,
        [Description("Stat EEG")]
        StatEEG = 13,
        [Description("Routine EEG")]
        RoutineEEG = 14,
        [Description("Long-term EEG")]
        LongTermEEG = 15,
        [Description("Routine Consult")]
        RoutineConsult = 16,
        [Description("Rounding New")]
        RoundingNew = 227,
        [Description("Rounding Follow-Up")]
        RoundingFollowUp = 228 ,
        [Description("Routine Consult- New")]
        RoutineConsultNew = 163,
        [Description("Routine Consult-Follow-Up")]
        RoutineConsultFollowUp = 164,
        [Description("UnKnown")]
        UnKnown = 220,
        [Description("Transfer Alert")]
        TransferAlert = 244,
        [Description("Test Results")]
        TestResults = 139,
        [Description("Radiology Call back")]
        RadiologyCallback = 226
        
    }

    public enum EntityTypes
    {
        [Description("Facility")]
        Facility = 1,
        [Description("User")]
        User = 2,
        StrokeAlertTemplateTpa = 3,
        NeuroStrokeAlertTemplateTpa = 4,
        StrokeAlertTemplateNoTpa = 5,
        StrokeAlertTemplateNoTpaTeleStroke = 6,
        [Description("Case")]
        Case = 7,
        [Description("Auto Save Case")]
        AutoSaveCase = 8,
        [Description("Credentials")]
        Credentials = 9,
        [Description("Sign Out Listing")]
        SignOutNotes = 10,
        StateAlertTemplate = 11,
    }
    public enum AcuteISchemicStatConsult
    {
        [Description("Rule Out Acute Ischemic Stroke")]
        ROAcuteIschemicStroke = 1,
        [Description("Seizure")]
        Seizure = 2,
        [Description("Encephalopathy")]
        Encephalopathy = 3,
        [Description("Transient Ischemic Attack")]
        TransientIschemicAttack = 4
    }
    public enum StatConsultCTHEAD
    {
        [Description("Showed No Acute Hemorrhage or Acute Core Infarct")]
        ShowedNoAcuteHemorrhageorAcuteCoreInfarct,
        [Description("Reviewed")]
        Reviewed,
        [Description("Not Reviewed")]
        NotReviewed
    }

    public enum CaseStatus
    {
        [Description("Open")]
        Open = 17,
        [Description("Waiting To Accept")]
        WaitingToAccept = 18,
        [Description("Accepted")]
        Accepted = 19,
        [Description("Complete")]
        Complete = 20,
        [Description("Cancelled")]
        Cancelled = 140
        //Pending = 2,
        //InQueue = 6
    }
    public enum BootStrapeAlertType
    {
        [Description("alert-success")]
        Success,
        [Description("alert-info")]
        Info,
        [Description("alert-warning")]
        Warning,
        [Description("alert-danger")]
        Danger

    }

    public enum FollowUpTypes
    {
        [Description("Follow-Up")]
        FollowUp,
        [Description("Sign Off")]
        SignOff
    }
    public enum ContractServiceTypes
    {
        [Description("TeleStroke")]
        TeleStroke,
        [Description("TeleNeuro")]
        TeleNeuro
    }

    public enum AcuteISchemicStroke
    {
        [Description("Rule Out Acute Ischemic Stroke")]
        ROAcuteIschemicStroke = 1,
        [Description("Large Vessel Infarct")]
        LargeVesselInfarct = 2,
        [Description("Small Vessel Infarct")]
        SmallVesselInfarct = 3,
        [Description("Right Hemispheric Infarct")] //TCARE-409
        Righthemispheric = 4,
        [Description("Left Hemispheric Infarct")] //TCARE-409
        Lefthemispheric = 5,
        [Description("MCA Distribution Infarct")] //TCARE-409
        MCAdistribution = 6,
        [Description("ACA Distribution Infarct")] //TCARE-409
        ACAdistribution = 7,
        [Description("PCA Distribution Infarct")] //TCARE-409
        PCAdistribution = 8,
        [Description("Anterior Circulation Infarct")] //TCARE-409
        AnteriorCirculation = 9,
        [Description("Posterior Circulation Infarct")] //TCARE-409
        PosteriorCirculation = 10,

        [Description("Transient Ischemic Attack")] //TCARE-409
        TransientIschemicAttack = 11
    }

    public enum StrokeMechanism
    {
        [Description("Possible Thromboembolic")]
        Thromboembolic,
        [Description("Possible Cardioembolic")]
        Cardioembolic,
        [Description("Small Vessel Disease")]
        SmallVesselDisease,
        [Description("Watershed Infarct")]
        WaterShedInfarct,
        [Description("Not Clear")]
        NotClear,

    }

    public enum TPAVerbalConsent
    {
        [Description("None")]
        None = 0,
        [Description("Patient")]
        Patient = 1,
        [Description("Family")]
        Family = 2,
        [Description("Guardian")]
        Guardian = 3

    }

    public enum TPABolusGiven
    {
        [Description("Without Complication")]
        WithoutComplication = 0,
        [Description("With Complication")]
        WithComplication = 1

    }

    public enum Gender
    {
        [Description("Male")]
        Male = 'M',
        [Description("Female")]
        Female = 'F'
    }

    public enum ComparisonOperator
    {
        [Description("Equal")]
        Equal,
        [Description("Greater Than")]
        GreaterThan,
        [Description("Greater Than Or Equal")]
        GreaterThanOrEqual,
        [Description("Less Than")]
        LessThan,
        [Description("Less Than Or Equal")]
        LessThanOrEqual,
        [Description("Not Equal")]
        NotEqual
    }

    public enum RPCMode
    {
        Disabled = 0,
        SignalR = 1,
        WebSocket = 2
    }

    public enum CaseBillingCode
    {
        [Description("CC1-Stroke Alert")]
        CC1_StrokeAlert = 1,
        [Description("CC1-STAT")]
        CC1_STAT = 2,
        [Description("New")]
        New = 3,
        [Description("FU")]
        FU = 4,
        [Description("EEG")]
        EEG = 5,
        [Description("LTM without Video (2-12 hrs)")]
        LTMEEG = 6,
        [Description("TC")]
        TC = 7,
        [Description("Not Seen")]
        NotSeen = 8,
        [Description("def")]
        def = 226,
        [Description("EEG (61-119 mins)")]
        EEG61119mins = 324,
        [Description("Stat EEG")]
        StatEEG = 325,
        [Description("LTM with Video (2-12 hrs)")]
        LTMWithVideo212hrs = 326,
    }

    public enum ThrombectomyMedicalDecisionMaking
    {
        [Description("Clinical Presentation is not Suggestive of Large Vessel Occlusive Disease")]
        ClinicalPresentationIsNotSuggestiveOfLargeVesselOcclusiveDisease_PatientIsNotACandidateForThrombectomy = 1,

        [Description("Lower Likelihood of Large Vessel Occlusion but Following Stat Studies are Recommended")]
        LowerLikelihoodOfLargeVesselOcclusiveButFollowingStatStudiesAreRecommended = 2,

        [Description("Clinical Presentation is Suggestive of Large Vessel Occlusive Disease, Recommendations are as Follows")]
        ClinicalPresentationIsSuggestiveOfLargeVesselOcclusiveDisease_RecommendationsAreAsFollows  = 3
    }

    #region Added by husnain 
    public enum IndexRate
    {
        Zero = 0,
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Eleven = 11,
        Twelve = 12,
        Thirteen = 13,
        Fourteen = 14,
        Fifteen = 15,
        Sixteen = 16,
        Seventeen = 17,
        Eighteen = 18,
        Ninteen = 19,
        Twenty = 20,
        TwentyOne = 21,
        TwentyTwo = 22,
        TwentyThree = 23,
        TwentyFour = 24,
        TwentyFive = 25,
        TwentySix = 26,
        TwentySeven = 27,
        TwentyEight = 28,
        TwentyNine = 29,
        Thirty = 30,
        ThirtyOne = 31,
        ThirtyTwo = 32,
        ThirtyThree = 33,
        ThirtyFour = 34,
        ThirtyFive = 35,
        ThirtySix = 36,
        ThirtySeven = 37,
        ThirtyEight = 38,
        ThirtyNine = 39,
        fourty = 40,
        fourtyOne = 41,
        fourtyTwo = 42,
        fourtyThree = 43,
        fourtyFour = 44,
        fourtyFive = 45,
        fourtySix = 46,
        fourtySeven = 47,
        fourtyEight = 48,
        fourtyNine = 49,
        fifty = 50
    }
    public enum PhysicianShifts
    {
        [Description("Day Shift")]
        DayShift = 1,
        [Description("Night Shift")]
        NightShift = 2,
        [Description("Moon Light")]
        MoonLight = 3,
        [Description("Day Light")]
        DayLight = 4,
        [Description("Day Shift Weekend")]
        DayShiftWeekend = 5,
        [Description("Night Shift Weekend")]
        NightShiftWeekend = 6,
        [Description("Blast")]
        Blast = 7,
        [Description("Moon Light Weekend")]
        MoonLightWeekend = 8,
        [Description("Day Light Weekend")]
        DayLightWeekend = 9,
        [Description("No Productivity")]
        NoProductivity = 10
    }
    public enum RCA
    {
        [Description("Arrival to start time delay")]
        ems_arivaltime = 200,
        [Description("Poor identification by EMS")]
        ems_pooridentification = 201,
        [Description("Acute LOC presentation for basilar thrombosis missed")]
        ems_acutelocpresentation = 202,
        [Description("Alternating symptoms presentation for basilar thrombosis missed")]
        ems_alternatingsymptoms = 203,
        [Description("Poor design or training of questions that would trigger tool usage (Defect)")]
        ems_poordesign = 204,
        [Description("Posterior Circulation symptoms (dizziness , nausea) missed")]
        ems_posterior = 205,
        [Description("Concurrent presentation / code (chest pain, trauma, sepsis alert)")]
        ems_concurrent = 206,
        [Description("Resolving or improving symptoms")]
        ems_resolving = 207,
        [Description("Identification occurred")]
        ems_identificationaccurred = 208,
        [Description("Pre-arrival activation by EMS did not take place")]
        ems_prearrivaltakeplace = 209,
        [Description("Pre-arrival activation by EMS took place, ED staff did not activate Stroke Alert")]
        ems_prearrivaled = 210,
        [Description("Pre-arrival activation by EMS took place, TS was not notified with ETA")]
        ems_prearrivaltswaseta = 211,
        [Description("ETA protocol was not carried out by TeleSpecialists RRC")]
        ems_etaprotocol = 212,
        [Description("No activation of stroke team")]
        ems_noactivation = 213,
        [Description("Activation of hospital stroke team took place, but poor preparedness of the team")]
        ems_activationofhospital = 214,
        [Description("Arrival to start time delay")]
        triage_arrivaltostart = 100,
        [Description("Triage Recognition")]
        triage_recognition = 101,
        [Description("Delays related to identification by Kiosk (Defect)")]
        triage_delaysrelated = 102,
        [Description("Pt ID defects- tool usage (Defect)")]
        triage_ptiddefects = 103,
        [Description("Poor design or training of questions that would trigger tool usage (Defect)")]
        triage_poordesign = 104,
        [Description("Inadequate skill set of front line staff")]
        triage_inadequateskill = 105,
        [Description("Poor design of the triage area (unnecessary wait)")]
        triage_poordesignunnecessarywait = 106,
        [Description("Stroke Alert Trigger")]
        triage_strokealert = 107,
        [Description("Triage unable to trigger stroke alert timely (Unnecessary wait)")]
        triage_unabletotriger = 108,
        [Description("EDMD needs to be notified before SA is triggered (Extra processing)")]
        triage_edmdneeds = 109,
        [Description("Absence of clear process for notification of TS when SA is triggered")]
        triage_absenceofclear = 110,
        [Description("NIHSS done before activation of TS (Extra processing)")]
        triage_nihssdone = 111,
        [Description("Transport and Rooming")]
        triage_transportandrooming = 112,
        [Description("Position of the cart unclear when stroke alert is called (Unnecessary wait)")]
        triage_positionofthecartunclear = 113,
        [Description("Position of the cart when teleneurologist on the screen (Unnecessary wait)")]
        triage_positionofthecarttleneurologist = 114,
        [Description("Transport to Room while the patient is at CT (Transportation)")]
        triage_transporttoroom = 115,
        [Description("Time First Log in attempt to NIHSS Start time delay")]
        inpatient_TimeFirstLoginattempt = 1,
        [Description("Delays related to imaging")]
        inpatient_delaysrelated = 11,
        [Description("Patient needed to be stabilized, examination was delayed")]
        inpatient_patientneeded = 12,
        [Description("Patient away from physician view for prolonged period of time")]
        inpatient_patientaway = 13,
        [Description("Cart left in room and not taken to CT scanner")]
        inpatient_cartleft = 14,
        [Description("Time First log in attempt to Video start time delay")]
        inpatient_timefirstlogin = 2,
        [Description("Physician unable to log in to cart")]
        inpatient_physicianunable = 21,
        [Description("Hospital network issue")]
        inpatient_hospitalnetwork = 22,
        [Description("No one responding to SA at the cart")]
        inpatient_nooneresponding = 23,
        [Description("Cart not on auto-answer")]
        inpatient_cartnot = 24,
        [Description("Physician station down")]
        inpatient_physicianstation = 25,
        [Description("Cart hardware malfunction")]
        inpatient_carthardware = 26,
        [Description("Arrival to Needle time delay")]
        inpatient_arrivaltoneedle = 3,
        [Description("Activation Delay")]
        inpatient_activationdelay = 31,
        [Description("Delays related to imaging")]
        inpatient_delaysrelatedtoimaging = 4,
        [Description("Waste related to unenhanced CT")]
        inpatient_wasterelatedtoCTAchestabdomenpelvistoruleoutdissection = 40,
        [Description("Waste related to CTA, chest abdomen pelvis to rule out dissection")]
        inpatient_WasterelatedtoCTAchestabdomenpelvistoruleoutdissection = 60,
        [Description("Delays in transport to CT (Rooming before the CT or early prolonged exam time before at EMS entrance area) (Unnecessary Wait)")]
        inpatient_delaysintransport = 41,
        [Description("CT table not ready (poor early activation and clearing of the table) (Defect)")]
        inpatient_cttablenotready = 42,
        [Description("Long distance to CT (Non-value added but necessary)")]
        inpatient_longdistance = 43,
        [Description("Technologist not ready for CT")]
        inpatient_technologistnotready = 44,
        [Description("No order for the CT, delay in early order (Unnecessary Wait)")]
        inpatient_noorderforthect = 45,
        [Description("Attempting to examine patient on the CT table by telemedicine, impossible and leads to increased wait")]
        inpatient_attemptingtoexamine = 46,
        [Description("Unavailable scaled stretcher to transport the patient to- after the CT (Unnecessary Wait)")]
        inpatient_unavailablescaled = 47,
        [Description("Waste related to advanced imaging")]
        inpatient_wasterelatedadvanced = 5,
        [Description("Delays related to IV site placement")]
        inpatient_delaysrelatedivsite = 50,
        [Description("Alteplase/Activase delayed due to staff request until second IV site placed  (Unnecessary wait)")]
        inpatient_tpadelays = 51,
        [Description("Delays with telemedicine assessment in room (teleneurology not taken to scanner)")]
        inpatient_delayswithtelemedicine = 52,
        [Description("Teleneurologist not taken to scanner and prolonged wait at the scanner for advanced imaging")]
        inpatient_teleneurologistnottaken = 53,
        [Description("Patient taken to advanced imaging prior to activation or presence of teleneurologist (Wait)")]
        inpatient_patienttakenadvanced = 54,
        [Description("Neurologist has no access to EMS")]
        inpatient_neurologisthasnoaccess = 55,
        [Description("Neurologist has no preliminary exam")]
        inpatient_neurologisthasnopreliminary = 56,
        [Description("Delays with telemedicine assessment in CT")]
        inpatient_delayswithtelemidicine = 57,
        [Description("Unavailable family for history and / or consent  (Unnecessary wait)")]
        inpatient_unavailablefamily = 58,
        [Description("Unavailable ED physician  (Unnecessary wait)")]
        inpatient_unavailableedphysician = 59,
        [Description("Centralized pharmacy challenges  (Unnecessary wait)")]
        inpatient_centeralizedpharmacy = 500,
        [Description("Teleneurologist exam  delayed until after imaging is completed due to staff request  (Unnecessary wait)")]
        inpatient_teleneurologistexamdelayedstaffrequest = 501,
        [Description("Teleneurologist exam delayed until after imaging is completed due to neurologist request  (Unnecessary wait)")]
        inpatient_teleneurologistexamdelayedneurologistrequest = 502,
        [Description("Teleneurologist exam delayed until after imaging is completed due to patient condition  (Unnecessary wait)")]
        inpatient_Teleneurologistexamdelayeduntilafter = 503,
        [Description("Teleneurologist taken to scanner and no Alteplase/Activase available at the scanner- bedside mixing and administration cannot be done until after all imaging is completed (Unnecessary wait)")]
        inpatient_Teleneurologisttakentoscanner = 504,
        [Description("BP management related delays")]
        inpatient_BPmanagementrelated = 6,
        [Description("Delayed detection of hypertension")]
        inpatient_Delayeddetectionofhypertension = 61,
        [Description("BP not available from EMS (Defect)")]
        inpatient_BPnotavailable = 62,
        [Description("BP not repeated upon presentation to the ED by triage, or primary nurse (defect)")]
        inpatient_BPnotrepeateduponpresentation = 63,
        [Description("Poor communication of hypertension early on (defect)")]
        inpatient_Poorcommunicationofhypertension = 64,
        [Description("BP not repeated at door upon EMS arrival (defect)")]
        inpatient_BPnotrepeatedatdoorupon = 65,
        [Description("No activation of early Labetalol or hydralazine order : staff waiting for clear orders from physician  (Unnecessary wait)")]
        inpatient_NoactivationofearlyLabetol = 66,
        [Description("Poor management of hypertension")]
        inpatient_Poormanagementofhypertension = 67,
        [Description("Antihypertensives not readily available (Defect)")]
        inpatient_Antihypertensivesnotreadily = 68,
        [Description("Poor communication of shortage of medicines to the team (Unnecessary Wait)")]
        inpatient_Poorcommunicationofshotage = 69,
        [Description("Staff searching for materials, e.g. infusion pump for cardene drip (Unnecessary Wait)")]
        inpatient_Staffsearchingformaterials = 600,
        [Description("Alteplase/Activase administration delays")]
        inpatient_TPAadministration = 7,
        [Description("Delays in workflow before mixing")]
        inpatient_Delaysinworkflow = 70,
        [Description("System")]
        inpatient_System = 71,
        [Description("Lack of Alteplase Early mixing / pre - mixing")]
        inpatient_LackofVerbalEarly = 72,
        [Description("Poor Buy-in of the pharmacy leadership")]
        inpatient_ = 73,
        [Description("Physician related")]
        inpatient_PoorBuypharmacy = 74,
        [Description("Poor Understanding / practice habit of the physician")]
        inpatient_PoorUnderstanding = 75,
        [Description("No use of LB2S2 for patient selection for early mix")]
        inpatient_NouseofLB2S2forpatient = 76,
        [Description("Centralized pharmacy delivery model waste")]
        inpatient_Centralizedpharmacydelivery = 77,
        [Description("Poor communication weight, dose, early mix (Defects)")]
        inpatient_Poorcommunicationweight = 78,
        [Description("Delays related to delivery of the medicine (Transportation)")]
        inpatient_Delaysrelatedtodelivery = 79,
        [Description("Misunderstanding of pharmacist on the actual order (premix vs verbal)")]
        inpatient_Misunderstandingofpharmacist = 80,
        [Description("Pharmacist/ RN insisting on CPOE order to mix/ pulling (Unnecessary Wait)")]
        inpatient_PharmacistRNinsistingCPOE = 81,
        [Description("Late Initiation of mixing the medicine at bedside or centralized pharmacy")]
        inpatient_LateInitiationmixingthemedicine = 82,
        [Description("Utilization of consent (Unnecessary Wait)")]
        inpatient_Utilizationofconsent = 83,
        [Description("Pharmacist/ bed side RN consenting the patient again after consent already obtained by physician")]
        inpatient_PharmacistbedsideRNconsenting = 84,
        [Description("Delays in mixing")]
        inpatient_Delaysinmixing = 85,
        [Description("Poor education and technique related to mixing (Defect)")]
        inpatient_Pooreducationandtechnique = 86,
        [Description("Turn over of nursing with poor orientation")]
        inpatient_Turnoverofnursing = 87,
        [Description("Delays in mixing related to poor staffing")]
        inpatient_Delaysinmixingrelated = 88,
        [Description("Centralized pharmacy model: poor staffing (esp after hours) one pharmacist to cover large scope particularly at night leading to extra queue time until staff available to mix Alteplase/Activase (Unnecessary Wait)")]
        inpatient_Centralizedpharmacymodel = 89,
        [Description("Delays in workflow after mixing")]
        inpatient_Delaysinworkflowafter = 9,
        [Description("Administration of bolus through the pump- (requires availability of the pump to administer bolus vs. drawing up bolus and administering it separately) (Unnecessary Wait)")]
        inpatient_Administrationofbolus = 91,
        [Description("Delays in setting up Alteplase/Activase infusion (Unnecessary Wait)")]
        inpatient_DelaysinsettinguptPA = 92,
        [Description("Delays in starting bolus related to unnecessary processes such as insertion of urinary catheter or second IV site prior to Alteplase/Activase bolus (Unnecessary Wait)- best practices")]
        inpatient_Delaysinstartingbolusrelated = 93,
        [Description("Delay related to placement of second IV site")]
        inpatient_elayrelatedtoplacement = 94,
        [Description("Medical Decision Making Delays")]
        inpatient_medicaldecisionmakingdelays = 8,
        [Description("Family not available / patient non-verbal")]
        inpatient_familynotavailableorpatientnonverbal = 800,
        [Description("Family / patient unsure of decision")]
        inpatient_familyorpatientunsureofdecision = 801,
        [Description("Delay in obtaining weight")]
        inpatient_Delayinobtainingweight = 900,
        [Description("Lack of weighted beds/equipment")]
        inpatient_Thenlackofweightedbedsorequipment = 901,
        [Description("Primary IV Access Issues")]
        inpatient_PrimaryIVAccessIssues = 96,
        [Description("Delays Related to lab")]
        inpatient_DelaysRelatedtolab = 10,
        [Description("Reporting of labs")]
        inpatient_Reportingoflabs = 1000,
        [Description("Delays in lab draw & delivery")]
        inpatient_Delaysinlabdrawanddelivery = 1001,
        [Description("Delays in lab processing")]
        inpatient_Delaysinlabprocessing = 1002,
        [Description("Delays in Reporting")]
        inpatient_DelaysinReporting = 1003,
        [Description("No phlebotomist available at bedside")]
        inpatient_Nophlebotomistavailableatbedside = 10011,
        [Description("Poor IV access")]
        inpatient_PoorIVaccess = 10012,
        [Description("Poor skillset / knowledge deficit")]
        inpatient_Poorskillsetorknowledgedeficit = 10013,
        [Description("Order not received")]
        inpatient_Ordernotreceived = 10014,
        [Description("Tube system delay")]
        inpatient_Tubesystemdelay = 10015,
        [Description("Poor hand-off / delivery")]
        inpatient_Poorhandoffordelivery = 10016,
        [Description("Knowledge deficit of lab staff of priority")]
        inpatient_Knowledgedeficitoflabstaffofpriority = 10020,
        [Description("Poor identification as STAT")]
        inpatient_PooridentificationasSTAT = 10021,
        [Description("Poor equipment")]
        inpatient_Poorequipment = 10022,
        [Description("Poor Stroke alert communication")]
        inpatient_PoorStrokealertcommunication = 10023,
    }
    public enum RootCauseTrends
    {
        PrimaryRootCause,
        SecondaryRootCause,
        TertiaryRootCause
    }
    public enum PrimaryRootCause
    {
        [Description("Time First Log in attempt to NIHSS Start time delay")]
        inpatient_TimeFirstLoginattempt = 1,
        [Description("Time First log in attempt to Video start time delay")]
        inpatient_timefirstlogin = 2,
        [Description("Arrival to Needle time delay")]
        inpatient_arrivaltoneedle = 3,
        [Description("Arrival to start time delay (Triage)")]
        triage_arrivaltostart = 100,
        [Description("Arrival to start time delay (EMS)")]
        ems_arivaltime = 200,
    }
    public enum SecondaryRootCause
    {
        [Description("Delays related to imaging")]
        inpatient_delaysrelated = 11,
        [Description("Patient needed to be stabilized, examination was delayed")]
        inpatient_patientneeded = 12,
        [Description("Patient away from physician view for prolonged period of time")]
        inpatient_patientaway = 13,
        [Description("Cart left in room and not taken to CT scanner")]
        inpatient_cartleft = 14,
        [Description("Physician unable to log in to cart")]
        inpatient_physicianunable = 21,
        [Description("Hospital network issue")]
        inpatient_hospitalnetwork = 22,
        [Description("No one responding to SA at the cart")]
        inpatient_nooneresponding = 23,
        [Description("Cart not on auto-answer")]
        inpatient_cartnot = 24,
        [Description("Physician station down")]
        inpatient_physicianstation = 25,
        [Description("Cart hardware malfunction")]
        inpatient_carthardware = 26,
        [Description("Activation Delay")]
        inpatient_activationdelay = 31,
        [Description("Delays related to imaging")]
        inpatient_delaysrelatedtoimaging = 4,
        [Description("BP management related delays")]
        inpatient_BPmanagementrelated = 6,
        [Description("Alteplase/Activase administration delays")]
        inpatient_TPAadministration = 7,
        [Description("Triage Recognition")]
        triage_recognition = 101,
        [Description("Stroke Alert Trigger")]
        triage_strokealert = 107,
        [Description("Transport and Rooming")]
        triage_transportandrooming = 112,
        [Description("Poor identification by EMS")]
        ems_pooridentification = 201,
        [Description("Identification occurred")]
        ems_identificationaccurred = 208,
        [Description("Delays Related to lab")]
        inpatient_DelaysRelatedtolab = 10,
        [Description("Medical Decision Making Delays")]
        inpatient_medicaldecisionmakingdelays = 8,
    }
    public enum TertiaryRootCause
    {
        [Description("Waste related to unenhanced CT")]
        inpatient_wasterelated = 40,
        [Description("Waste related to CTA, chest abdomen pelvis to rule out dissection")]
        inpatient_WasterelatedtoCTAchestabdomenpelvistoruleoutdissection = 60,
        [Description("Delays in transport to CT (Rooming before the CT or early prolonged exam time before at EMS entrance area) (Unnecessary Wait)")]
        inpatient_delaysintransport = 41,
        [Description("CT table not ready (poor early activation and clearing of the table) (Defect)")]
        inpatient_cttablenotready = 42,
        [Description("Long distance to CT (Non-value added but necessary)")]
        inpatient_longdistance = 43,
        [Description("Technologist not ready for CT")]
        inpatient_technologistnotready = 44,
        [Description("No order for the CT, delay in early order (Unnecessary Wait)")]
        inpatient_noorderforthect = 45,
        [Description("Attempting to examine patient on the CT table by telemedicine, impossible and leads to increased wait")]
        inpatient_attemptingtoexamine = 46,
        [Description("Unavailable scaled stretcher to transport the patient to- after the CT (Unnecessary Wait)")]
        inpatient_unavailablescaled = 47,
        [Description("Waste related to advanced imaging")]
        inpatient_wasterelatedadvanced = 5,
        [Description("Delays related to IV site placement")]
        inpatient_delaysrelatedivsite = 50,
        [Description("Alteplase/Activase delayed due to staff request until second IV site placed  (Unnecessary wait)")]
        inpatient_tpadelays = 51,
        [Description("Delays with telemedicine assessment in room (teleneurology not taken to scanner)")]
        inpatient_delayswithtelemedicine = 52,
        [Description("Teleneurologist not taken to scanner and prolonged wait at the scanner for advanced imaging")]
        inpatient_teleneurologistnottaken = 53,
        [Description("Patient taken to advanced imaging prior to activation or presence of teleneurologist (Wait)")]
        inpatient_patienttakenadvanced = 54,
        [Description("Neurologist has no access to EMS")]
        inpatient_neurologisthasnoaccess = 55,
        [Description("Neurologist has no preliminary exam")]
        inpatient_neurologisthasnopreliminary = 56,
        [Description("Delays with telemedicine assessment in CT")]
        inpatient_delayswithtelemidicine = 57,
        [Description("Unavailable family for history and / or consent  (Unnecessary wait)")]
        inpatient_unavailablefamily = 58,
        [Description("Unavailable ED physician  (Unnecessary wait)")]
        inpatient_unavailableedphysician = 59,
        [Description("Centralized pharmacy challenges  (Unnecessary wait)")]
        inpatient_centeralizedpharmacy = 500,
        [Description("Teleneurologist exam  delayed until after imaging is completed due to staff request  (Unnecessary wait)")]
        inpatient_teleneurologistexamdelayedstaffrequest = 501,
        [Description("Teleneurologist exam delayed until after imaging is completed due to neurologist request  (Unnecessary wait)")]
        inpatient_teleneurologistexamdelayedneurologistrequest = 502,
        [Description("Teleneurologist exam delayed until after imaging is completed due to patient condition  (Unnecessary wait)")]
        inpatient_Teleneurologistexamdelayeduntilafter = 503,
        [Description("Teleneurologist taken to scanner and no Alteplase/Activase available at the scanner- bedside mixing and administration cannot be done until after all imaging is completed (Unnecessary wait)")]
        inpatient_Teleneurologisttakentoscanner = 504,
        [Description("Delayed detection of hypertension")]
        inpatient_Delayeddetectionofhypertension = 61,
        [Description("BP not available from EMS (Defect)")]
        inpatient_BPnotavailable = 62,
        [Description("BP not repeated upon presentation to the ED by triage, or primary nurse (defect)")]
        inpatient_BPnotrepeateduponpresentation = 63,
        [Description("Poor communication of hypertension early on (defect)")]
        inpatient_Poorcommunicationofhypertension = 64,
        [Description("BP not repeated at door upon EMS arrival (defect)")]
        inpatient_BPnotrepeatedatdoorupon = 65,
        [Description("No activation of early Labetalol or hydralazine order : staff waiting for clear orders from physician  (Unnecessary wait)")]
        inpatient_NoactivationofearlyLabetol = 66,
        [Description("Poor management of hypertension")]
        inpatient_Poormanagementofhypertension = 67,
        [Description("Antihypertensives not readily available (Defect)")]
        inpatient_Antihypertensivesnotreadily = 68,
        [Description("Poor communication of shortage of medicines to the team (Unnecessary Wait)")]
        inpatient_Poorcommunicationofshotage = 69,
        [Description("Staff searching for materials, e.g. infusion pump for cardene drip (Unnecessary Wait)")]
        inpatient_Staffsearchingformaterials = 600,
        [Description("Delays in workflow before mixing")]
        inpatient_Delaysinworkflow = 70,
        [Description("System")]
        inpatient_System = 71,
        [Description("Lack of Alteplase Early mixing / pre - mixing")]
        inpatient_LackofVerbalEarly = 72,
        [Description("Poor Buy-in of the pharmacy leadership")]
        inpatient_ = 73,
        [Description("Physician related")]
        inpatient_PoorBuypharmacy = 74,
        [Description("Poor Understanding / practice habit of the physician")]
        inpatient_PoorUnderstanding = 75,
        [Description("No use of LB2S2 for patient selection for early mix")]
        inpatient_NouseofLB2S2forpatient = 76,
        [Description("Centralized pharmacy delivery model waste")]
        inpatient_Centralizedpharmacydelivery = 77,
        [Description("Poor communication weight, dose, early mix (Defects)")]
        inpatient_Poorcommunicationweight = 78,
        [Description("Delays related to delivery of the medicine (Transportation)")]
        inpatient_Delaysrelatedtodelivery = 79,
        [Description("Misunderstanding of pharmacist on the actual order (premix vs verbal)")]
        inpatient_Misunderstandingofpharmacist = 80,
        [Description("Pharmacist/ RN insisting on CPOE order to mix/ pulling (Unnecessary Wait)")]
        inpatient_PharmacistRNinsistingCPOE = 81,
        [Description("Late Initiation of mixing the medicine at bedside or centralized pharmacy")]
        inpatient_LateInitiationmixingthemedicine = 82,
        [Description("Utilization of consent (Unnecessary Wait)")]
        inpatient_Utilizationofconsent = 83,
        [Description("Pharmacist/ bed side RN consenting the patient again after consent already obtained by physician")]
        inpatient_PharmacistbedsideRNconsenting = 84,
        [Description("Delays in mixing")]
        inpatient_Delaysinmixing = 85,
        [Description("Poor education and technique related to mixing (Defect)")]
        inpatient_Pooreducationandtechnique = 86,
        [Description("Turn over of nursing with poor orientation")]
        inpatient_Turnoverofnursing = 87,
        [Description("Delays in mixing related to poor staffing")]
        inpatient_Delaysinmixingrelated = 88,
        [Description("Centralized pharmacy model: poor staffing (esp after hours) one pharmacist to cover large scope particularly at night leading to extra queue time until staff available to mix Alteplase/Activase (Unnecessary Wait)")]
        inpatient_Centralizedpharmacymodel = 89,
        [Description("Delays in workflow after mixing")]
        inpatient_Delaysinworkflowafter = 9,
        [Description("Administration of bolus through the pump- (requires availability of the pump to administer bolus vs. drawing up bolus and administering it separately) (Unnecessary Wait)")]
        inpatient_Administrationofbolus = 91,
        [Description("Delays in setting up Alteplase/Activase infusion (Unnecessary Wait)")]
        inpatient_DelaysinsettinguptPA = 92,
        [Description("Delays in starting bolus related to unnecessary processes such as insertion of urinary catheter or second IV site prior to Alteplase/Activase bolus (Unnecessary Wait)- best practices")]
        inpatient_Delaysinstartingbolusrelated = 93,
        [Description("Delay related to placement of second IV site")]
        inpatient_elayrelatedtoplacement = 94,
        [Description("Transport and Rooming")]
        inpatient_transportandrooming = 95,
        [Description("Delays related to identification by Kiosk (Defect)")]
        triage_delaysrelated = 102,
        [Description("Pt ID defects- tool usage (Defect)")]
        triage_ptiddefects = 103,
        [Description("Poor design or training of questions that would trigger tool usage (Defect)")]
        triage_poordesign = 104,
        [Description("Inadequate skill set of front line staff")]
        triage_inadequateskill = 105,
        [Description("Poor design of the triage area (unnecessary wait)")]
        triage_poordesignunnecessarywait = 106,
        [Description("Triage unable to trigger stroke alert timely (Unnecessary wait)")]
        triage_unabletotriger = 108,
        [Description("EDMD needs to be notified before SA is triggered (Extra processing)")]
        triage_edmdneeds = 109,
        [Description("Absence of clear process for notification of TS when SA is triggered")]
        triage_absenceofclear = 110,
        [Description("NIHSS done before activation of TS (Extra processing)")]
        triage_nihssdone = 111,
        [Description("Position of the cart unclear when stroke alert is called (Unnecessary wait)")]
        triage_positionofthecartunclear = 113,
        [Description("Position of the cart when teleneurologist on the screen (Unnecessary wait)")]
        triage_positionofthecarttleneurologist = 114,
        [Description("Transport to Room while the patient is at CT (Transportation)")]
        triage_transporttoroom = 115,
        [Description("Acute LOC presentation for basilar thrombosis missed")]
        ems_acutelocpresentation = 202,
        [Description("Alternating symptoms presentation for basilar thrombosis missed")]
        ems_alternatingsymptoms = 203,
        [Description("Poor design or training of questions that would trigger tool usage (Defect)")]
        ems_poordesign = 204,
        [Description("Posterior Circulation symptoms (dizziness , nausea) missed")]
        ems_posterior = 205,
        [Description("Concurrent presentation / code (chest pain, trauma, sepsis alert)")]
        ems_concurrent = 206,
        [Description("Resolving or improving symptoms")]
        ems_resolving = 207,
        [Description("Pre-arrival activation by EMS did not take place")]
        ems_prearrivaltakeplace = 209,
        [Description("Pre-arrival activation by EMS took place, ED staff did not activate Stroke Alert")]
        ems_prearrivaled = 210,
        [Description("Pre-arrival activation by EMS took place, TS was not notified with ETA")]
        ems_prearrivaltswaseta = 211,
        [Description("ETA protocol was not carried out by TeleSpecialists RRC")]
        ems_etaprotocol = 212,
        [Description("No activation of stroke team")]
        ems_noactivation = 213,
        [Description("Activation of hospital stroke team took place, but poor preparedness of the team")]
        ems_activationofhospital = 214,
        [Description("Reporting of labs")]
        inpatient_Reportingoflabs = 1000,
        [Description("Delays in lab draw & delivery")]
        inpatient_Delaysinlabdrawanddelivery = 1001,
        [Description("Delays in lab processing")]
        inpatient_Delaysinlabprocessing = 1002,
        [Description("Delays in Reporting")]
        inpatient_DelaysinReporting = 1003,
        [Description("No phlebotomist available at bedside")]
        inpatient_Nophlebotomistavailableatbedside = 10011,
        [Description("Poor IV access")]
        inpatient_PoorIVaccess = 10012,
        [Description("Poor skillset / knowledge deficit")]
        inpatient_Poorskillsetorknowledgedeficit = 10013,
        [Description("Order not received")]
        inpatient_Ordernotreceived = 10014,
        [Description("Tube system delay")]
        inpatient_Tubesystemdelay = 10015,
        [Description("Poor hand-off / delivery")]
        inpatient_Poorhandoffordelivery = 10016,
        [Description("Knowledge deficit of lab staff of priority")]
        inpatient_Knowledgedeficitoflabstaffofpriority = 10020,
        [Description("Poor identification as STAT")]
        inpatient_PooridentificationasSTAT = 10021,
        [Description("Poor equipment")]
        inpatient_Poorequipment = 10022,
        [Description("Poor Stroke alert communication")]
        inpatient_PoorStrokealertcommunication = 10023,
        [Description("Family not available / patient non-verbal")]
        inpatient_familynotavailableorpatientnonverbal = 800,
        [Description("Family / patient unsure of decision")]
        inpatient_familyorpatientunsureofdecision = 801,
        [Description("Delay in obtaining weight")]
        inpatient_Delayinobtainingweight = 900,
        [Description("Lack of weighted beds/equipment")]
        inpatient_Thenlackofweightedbedsorequipment = 901,
        [Description("Primary IV Access Issues")]
        inpatient_PrimaryIVAccessIssues = 96,
    }
    public enum PacCaseType
    {
        [Description("PAC New Patient Consult")]
        PACNewpatientconsult = 1,
        [Description("PAC RN Phone Call")]
        PACRNPhonecall = 2,
        [Description("PAC Follow Up Consult")]
        PACFollowupconsult = 3,
        [Description("PAC Urgent Consult")]
        PACUrgentconsult = 4,
        [Description("Sleep Request")] // on uat
        SleepRequest = 334
        //[Description("PAC Consult Request")] // on local
        //PACConsultRequest = 230
    }

    public enum PacStatus
    {
        [Description("Open")]//for uat
        Open = 238,
        [Description("Assigned")]
        Assigned = 239,
        [Description("Confirmed")]
        Confirmed = 240,
        [Description("Complete")]
        Complete = 241,
        [Description("Cancelled")]
        Cancelled = 242
        //[Description("Open")] // for local
        //Open = 225,
        //[Description("Assigned")]
        //Assigned = 226,
        //[Description("Confirmed")]
        //Confirmed = 227,
        //[Description("Complete")]
        //Complete = 228,
        //[Description("Cancelled")]
        //Cancelled = 229
    }
    public enum PACIdentificationTypes
    {
        [Description("CSN")]
        CSN = 159,
        [Description("MRN")]
        MRN = 160,
        [Description("FIN")]
        FIN = 161,
        [Description("Account Number")]
        AccountNumber = 162
    }

    public enum PACAppears
    {
        [Description("stated age")]
        statedage = 1,
        [Description("older than stated age")]
        olderstated = 2,
        [Description("younger than stated age")]
        youngerstated = 3
    }
    public enum PACHeentFirst
    {
        [Description("PERRL")]
        PERRL = 1,
        [Description("Unequal pupils")]
        Unequal = 2
    }
    public enum PACHeentSecond
    {
        [Description("EOEM Intact")]
        intact = 1,
        [Description("EOEM Impaired")]
        impaired = 2
    }
    public enum PACPresentAbsent
    {
        [Description("Present")]
        present = 1,
        [Description("Absent")]
        absent = 2
    }
    public enum PACGI
    {
        [Description("Distended")]
        distented = 1,
        [Description("Non distended")]
        nondistended = 2
    }
    public enum PACRaneofMotion
    {
        [Description("Normal")]
        normal = 1,
        [Description("Abnormal")]
        abnormal = 2
    }
    public enum PACNeuroFace
    {
        [Description("Symmetric")]
        symmetric = 1,
        [Description("Facial Droop")]
        facialdroop = 2
    }
    public enum PACNeuroSpeech
    {
        [Description("Fluent")]
        symmetric = 1,
        [Description("Dysarthric")]
        dysarthric = 2
    }

    #region CheckBox Lists
    public enum StatGeneral
    {
        [Description("Alert,Awake")]
        AlertAwake = 0,
        [Description("Oriented")]
        Oriented = 1,
        [Description("Time")]
        Time = 2,
        [Description("Place")]
        Place = 3,
        [Description("Person")]
        Person = 4
    }
    public enum StatConsultFace
    {
        [Description("Right")]
        Right = 5,
        [Description("Left")]
        Left = 6,
        [Description("LMN Type")]
        LMNType = 7,
    }
    public enum StatConsultFacialSensation
    {
        [Description("Right")]
        Right = 8,
        [Description("Left")]
        Left = 9
    }
    public enum StatConsultMotor
    {
        [Description("RUE")]
        RUE = 10,
        [Description("RLE")]
        RLE = 11,
        [Description("LUE")]
        LUE = 12,
        [Description("LLE")]
        LLE = 13
    }
    public enum StatConsultVisual
    {
        [Description("Right")]
        Right = 14,
        [Description("Left")]
        Left = 15
    }
    public enum StatConsultExtra
    {
        [Description("Right")]
        Right = 16,
        [Description("Left")]
        Left = 17
    }
    public enum StatConsultSensation
    {
        [Description("RUE")]
        RUE = 18,
        [Description("RLE")]
        RLE = 19,
        [Description("LUE")]
        LUE = 20,
        [Description("LLE")]
        LLE = 21
    }
    public enum StatConsultCoordination
    {
        [Description("RUE")]
        RUE = 22,
        [Description("RLE")]
        RLE = 23,
        [Description("LUE")]
        LUE = 24,
        [Description("LLE")]
        LLE = 25
    }



    #endregion

    #region Radio Button Lists

    public enum StatSpeech
    {
        [Description("Fluent")]
        Fluent = 'F',
        [Description("Dysarthric")]
        Dysarthric = 'D'
    }
    public enum StatLanguage
    {
        [Description("Intact")]
        Intact = 'N',
        [Description("Mild Aphasia")]
        MildAphasia = 'M',
        [Description("Aphasia")]
        Aphasia = 'A',
    }
    public enum StatFace
    {
        [Description("Symmetric")]
        Symmetric = 'S',
        [Description("Facial Droop")]
        FacialDroop = 'F'
    }
    public enum StatFacialSensation
    {
        [Description("Intact")]
        Intact = 'I',
        [Description("Reduced")]
        Reduced = 'R'
    }
    public enum StatVisual
    {
        [Description("Intact")]
        Intact = 'R',
        [Description("Hemianopsia")]
        Hemianopsia = 'H',
        [Description("Quadrantanospia")]
        Quadrantanospia = 'Q',
    }
    public enum StatExtraocular
    {
        [Description("Intact")]
        Intact = 'I',
        [Description("Gaze Preference")]
        GazePreference = 'G',
    }
    public enum StatMotor
    {
        [Description("No Drift")]
        NoDrift = 'N',
        [Description("Drift")]
        Drift = 'Y',
    }
    public enum StatSensation
    {
        [Description("Intact")]
        Intact = 'I',
        [Description("Reduced")]
        Reduced = 'R',
    }
    public enum StatCoordination
    {
        [Description("Intact")]
        Intact = 'I',
        [Description("Dysmetria")]
        Dysmetria = 'D',
    }
    #endregion

    public enum ChartColors
    {
        [Description("#3690c1")]
        Blue = 1,
        [Description("#b4d1e6")]
        SoftBlue = 2,
        [Description("#808f97")]
        Grayish = 3,
        [Description("#249ebf")]
        Teal = 4,
        [Description("#66338e")]
        Purple = 5
    }
    public enum MonthName
    {
        [Description("January")]
        January = 1,
        [Description("February")]
        February = 2,
        [Description("March")]
        March = 3,
        [Description("April")]
        April = 4,
        [Description("May")]
        May = 5,
        [Description("June")]
        June = 6,
        [Description("July")]
        July = 7,
        [Description("August")]
        August = 8,
        [Description("September")]
        September = 9,
        [Description("October")]
        October = 10,
        [Description("November")]
        November = 11,
        [Description("December")]
        December = 12
    }


    #endregion
    #region Added by Ahmad
    public enum QPSSelected
    {
        [Description("Danielle DeCubellis")]
        Danielle_DeCubellis = 1,
        [Description("Micaela Prevatke")]
        Micaela_Prevatke = 2,
        [Description("Haley Fogle")]
        Haley_Fogle = 3,
        [Description("Gayle Obrien")]
        Gayle_Obrien = 4,
        [Description("Lauren Voltz")]
        Lauren_Voltz = 5,
        [Description("Michelle McWhorter")]
        Michelle_McWhorter = 6,
        [Description("Kristy Reese")]
        Kristy_Reese = 7,
    }
    #endregion

    #region Added by Axim
    public enum CTHeadReviewPersonally
    {
        [Description("-- Select --")]
        Select = 0,
        [Description("Could not access imaging")]
        Could_not_access_imaging = 1,
        [Description("Hospital system down")]
        Hospital_system_down = 2,
        [Description("Other")]
        Other = 3,
    }
    public enum AdvancedImagingPersonally
    {
        [Description("-- Select --")]
        Select = 0,
        [Description("No access to order entry")]
        No_access_to_order_entry = 1,
        [Description("Not indicated")]
        Not_indicated = 2,
        [Description("Already ordered by ED MD")]
        EDMD = 3,
        [Description("Other")]
        Other = 4,
    }
    public enum tPAORderedPersonally
    {
        [Description("-- Select --")]
        Select = 0,
        [Description("No access to order entry")]
        No_access_to_order_entry = 1,
        [Description("Order already placed when I accessed")]
        Order_already_placed_when_I_accessed = 2,
        [Description("Other")]
        Other = 3,
    }
    public enum VizAppUsagePersonally
    {
        [Description("-- Select --")]
        Select = 0,
        [Description("I was unable to access VIZ app")]
        No_access_to_order_entry = 1,
        [Description("I do not have access to VIZ app")]
        order_already_placed_when_I_accessed = 2,
        [Description("I do not have training for VIZ app")]
        I_do_not_have_training_for_VIZ_app = 3,
        [Description("I am not sure how to use VIZ app")]
        I_am_not_sure_how_to_use_VIZ_app = 4,
        [Description("Other")]
        Other = 5
    }
    public enum NIHSSPatientsComatose {
        [Description("-- Select --")]
        Select = 0,
        [Description("Patient is comatose")]
        patient_comatose = 1,
        [Description("Patient was intubated and paralyzed")]
        patient_intubated = 2,
        [Description("Other")]
        other = 3,

    }

    #endregion
}

