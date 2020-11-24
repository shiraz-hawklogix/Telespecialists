using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{

    using BLL.Extensions;
    using System.Data.Entity.Infrastructure;

    public class UnitOfWork : IDisposable
    {
        private readonly TeleSpecialistsContext context;
        private DbContextTransaction _transactionScope;

        public UnitOfWork()
        {
            this.context = new TeleSpecialistsContext();
        }
        public void BeginTransaction()
        {
            this._transactionScope = this.context.Database.BeginTransaction();
        }
        public void Rollback()
        {
            this._transactionScope?.Rollback();
        }
        public void Commit()
        {
            this._transactionScope?.Commit();
        }
        public void DetectChanges()
        {
            this.context.ChangeTracker.DetectChanges();
        }
        public void SetChangeTrackiing(bool enabled)
        {
            this.context.Configuration.AutoDetectChangesEnabled = enabled;
        }
        public List<T> ExecuteStoreProcedure<T>(string sql, params object[] parameters)
        {
            return this.context.Database.SqlQuery<T>(sql, parameters).ToList();
        }


        #region ----- Inline Query  -----
        public List<Dictionary<string, object>> SqlToList(string query)
        {
            return this.context.SqlToList(query, this._transactionScope?.UnderlyingTransaction);
        }
        public List<T> LoadDashboardCharts<T>(string userId)
        {
            return this.context.Database.SqlQuery<T>("Exec sp_chart_for_dashboard '{0}'", userId).ToList();
        }
        public List<T> SqlQuery<T>(string query)
        {
            return this.context.Database.SqlQuery<T>(query).ToList();
        }
        public int ExecuteSqlCommand(string query, params object[] parameters)
        {
            return this.context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, query, parameters);
        }
        public async Task<int> ExecuteSqlCommandAsync(string query, params object[] parameters)
        {
            return await this.context.Database.ExecuteSqlCommandAsync(TransactionalBehavior.DoNotEnsureTransaction, query, parameters);
        }

        public List<DbEntityEntry> GetChangeSet()
        {
            return this.context.ChangeTracker
                               .Entries().ToList();
                               
        }
        #endregion

        #region ----- Repositories -----

        private IQueryable<AspNetUser> _applicationUsers;
        private IQueryable<AspNetRole> _applicationRoles;
        private IQueryable<AspNetUserRole> _applicationUserRoles;

        private ICaseRepository _caseRepository;    
        private IFacilityRepository _facilityRepository;

        private ICwhData _cwhRepository;
        private IHospitalProtocols __HospitalProtocols;

        private IPhysicianStatusRepository _physicianStatusRepository;
        private IContactRepository _contactRepository;
        private IFacilityPhysicianRepository _facilityPhysicianRepository;
        private IPhysicianStatusLogRepository _physicianStatusLogRepository;
        private IFacilityContractRepository _facilityContractRepository;
        private IFacilityContractServiceRepository _facilityContractServiceRepository;
        private IEntityNoteRepository _facilityNoteRepository;

        private IStrokeCertificationRepository _strokeCertificationRepository;
        private IPhysicianLicenseRepository _physicianLicenseRepository;
        private ICallHistoryRepository _callHistoryRepository;
        private IAspNetUsersLogRepository _aspNetUsersLogRepository;
        private IAspNetUsersPasswordResetRepository _aspNetUsersPasswordResetRepository;
        private ICaseAssignHistoryRepository _caseAssignHistoryRepository;
        private IAppSettingRepository _appSettingRepository;
        private IScheduleRepository _scheduleRepository;

        private IUserRepository _userRepository;
        private IUserRoleRepository _userRoleRepository;
        private IRoleRepository _roleRepository;

        private IUCLRepository _uclRepository;
        private IUCL_UCDRepository uCL_UCDRepository;
        private Iphysician_status_snoozeRepository _physician_Status_SnoozeRepository;
        private ICaseCopyLogRepository _caseCopyLogRepository;

        private INIHStrokeScaleQuestionRepository _nIHStrokeScaleQuestionRepository;
        private INIHStrokeScaleRepository _nIHStrokeScaleRepository;
        private IEAlertCaseTypesRepository _eAlertCaseTypesRepository;
        private IEAlertFacilitiesRepository _eAlertFacilitiesRepository;
        private INIHStrokeScaleAnswerRepository _nIHStrokeScaleAnswerRepository;
        private ICaseGeneratedTemplateRepository _caseGeneratedTemplateRepository;
        private ICaseTemplateStrokeTpaRepository _caseTemplateStrokeTpaRepository; 
        private ICaseTemplateStrokeNoTpaRepository _caseTemplateStrokeNoTpaRepository;
        private ICaseTemplateStrokeNeuroTPARepository _caseTemplateStrokeNeuroTPARepository;
        private ICaseTemplateTelestrokeNotpaRepository _caseTemplateTelestrokeNotpaRepository;
        private IPhysicianStatusSnoozeOptionRepository _physicianStatusSnoozeOptionRepository;
        private IAspNetUserDetailRepositorty _aspNetUserDetailRepositorty;
        private IEntityChangeLogRepository _entityChangeLogRepository;

        private IWeb2Campaign_LogRepository _web2CampaignLogRepository;
        private IRapidsMailboxRepository _rapidsMailboxRepository;
        private IPhysician_Case_TempRepository _physician_Case_TempRepository;

        private IAuditRecordsRepository _audit_record_Repository;
        private IFacilityQuestionnairePreLiveRepository _facilityQuestionnairePreLiveRepository;
        private IFacilityQuestionnaireContactDesignationRepository _facilityQuestionnaireContactDesignationRepository;
        private IFacilityQuestionnaireContactRespository _facilityQuestionnaireContactRespository;
        private IFacilityRateRepository _facilityRateRepository;
        private IFacilityAvailabilityRateRepository _facilityAvailabilityRateRepository;

        //private IMenuData _menuRepository;
        //private IMenuAccessData _menuAccessRepository;
        private IMockCaseRepository _mockCaseRepository;
        //private IUserAccessData _userAccessRightsRepository;

        public IQueryable<AspNetUser> ApplicationUsers
        {
            get
            {
                if (this._applicationUsers == null)
                {
                    this._applicationUsers = context.AspNetUsers;
                }
                return _applicationUsers;
            }


        }

        public IQueryable<AspNetRole> ApplicationRoles
        {
            get
            {
                if (this._applicationRoles == null)
                {
                    this._applicationRoles = context.AspNetRoles;
                }
                return _applicationRoles;
            }
        }
        
        public IQueryable<AspNetUserRole> ApplicationUserRoles
        {
            get
            {
                if (this._applicationUserRoles == null)
                {
                    this._applicationUserRoles = context.AspNetUserRoles;
                }
                return _applicationUserRoles;
            }
        }
        

        public IAuditRecordsRepository AuditRecordsRepository
        {
            get
            {
                if (this._audit_record_Repository == null)
                {
                    this._audit_record_Repository = new AuditRecordsRepository(context);
                }

                return this._audit_record_Repository;
            }
        }

        public ICaseRepository CaseRepository
        {
            get
            {
                if (this._caseRepository == null)
                {
                    this._caseRepository = new CaseRepository(context);
                }
                return _caseRepository;
            }
        }

        public ICallHistoryRepository CallHistoryRepository
        {
            get
            {
                if (this._callHistoryRepository == null)
                {
                    this._callHistoryRepository = new CallHistoryRepository(context);
                }
                return _callHistoryRepository;
            }
        }

        public IAspNetUsersLogRepository AspNetUsersLogRepository
        {
            get
            {
                if (this._aspNetUsersLogRepository == null)
                {
                    this._aspNetUsersLogRepository = new AspNetUsersLogRepository(context);
                }
                return _aspNetUsersLogRepository;
            }
        }

        public IAspNetUsersPasswordResetRepository AspNetUsersPasswordResetRepository
        {
            get
            {
                if (this._aspNetUsersPasswordResetRepository == null)
                {
                    this._aspNetUsersPasswordResetRepository = new AspNetUsersPasswordResetRepository(context);
                }
                return _aspNetUsersPasswordResetRepository;
            }
        }
        //public IMenuData MenuRepository
        //{
        //    get
        //    {
        //        if (this._menuRepository == null)
        //        {
        //            this._menuRepository = new MenuRepository(context);
        //        }
        //        return _menuRepository;
        //    }
        //}
        //public IMenuAccessData MenuAccessRepository
        //{
        //    get
        //    {
        //        if (this._menuAccessRepository == null)
        //        {
        //            this._menuAccessRepository = new MenuAccessRepository(context);
        //        }
        //        return _menuAccessRepository;
        //    }
        //} 
        //public IUserAccessData UserAccessRepository
        //{
        //    get
        //    {
        //        if (this._userAccessRightsRepository == null)
        //        {
        //            this._userAccessRightsRepository = new UserAccessRepository(context);
        //        }
        //        return _userAccessRightsRepository;
        //    }
        //}
        public IFacilityRepository FacilityRepository
        {
            get
            {
                if (this._facilityRepository == null)
                {
                    this._facilityRepository = new FacilityRepository(context);
                }
                return _facilityRepository;
            }
        }
        #region Shiraz Code For OnBoarded   
        private IOnBoardedRepository _OnBoardedRepository;
        public IOnBoardedRepository OnBoardedRepository
        {
            get
            {
                if (this._OnBoardedRepository == null)
                {
                    this._OnBoardedRepository = new OnBoardedRepository(context);
                }
                return _OnBoardedRepository;
            }
        }
        #endregion
        public IFacilityRateRepository facilityRateRepository
        {
            get
            {
                if (this._facilityRateRepository == null)
                {
                    this._facilityRateRepository = new FacilityRateRepository(context);
                }
                return _facilityRateRepository;
            }
        }
        public IFacilityAvailabilityRateRepository facilityAvailabilityRateRepository
        {
            get
            {
                if (this._facilityAvailabilityRateRepository == null)
                {
                    this._facilityAvailabilityRateRepository = new FacilityAvailabilityRateRepository(context);
                }
                return _facilityAvailabilityRateRepository;
            }
        }
        public IPhysicianStatusRepository PhysicianStatusRepository
        {
            get
            {
                if (this._physicianStatusRepository == null)
                {
                    this._physicianStatusRepository = new PhysicianStatusRepository(context);
                }
                return _physicianStatusRepository;
            }
        }

        public IContactRepository ContactRepository
        {
            get
            {
                if (this._contactRepository == null)
                {
                    this._contactRepository = new ContactRepository(context);
                }
                return _contactRepository;
            }
        }

        public IFacilityPhysicianRepository FacilityPhysicianRepository
        {
            get
            {
                if (this._facilityPhysicianRepository == null)
                {
                    this._facilityPhysicianRepository = new FacilityPhysicianRepository(context);
                }
                return _facilityPhysicianRepository;
            }
        }

        public IPhysicianStatusLogRepository PhysicianStatusLogRepository
        {
            get
            {
                if (this._physicianStatusLogRepository == null)
                {
                    this._physicianStatusLogRepository = new PhysicianStatusLogRepository(context);
                }
                return _physicianStatusLogRepository;
            }
        }

        public IFacilityContractRepository FacilityContractRepository
        {
            get
            {
                if (this._facilityContractRepository == null)
                {
                    this._facilityContractRepository = new FacilityContractRepository(context);
                }
                return _facilityContractRepository;
            }
        }

        public IFacilityContractServiceRepository FacilityContractServiceRepository
        {
            get
            {
                if (this._facilityContractServiceRepository == null)
                {
                    this._facilityContractServiceRepository = new FacilityContractServiceRepository(context);
                }
                return _facilityContractServiceRepository;
            }
        }

        public IEntityNoteRepository EntityNoteRepository
        {
            get
            {
                if (this._facilityNoteRepository == null)
                {
                    this._facilityNoteRepository = new EntityNoteRepository(context);
                }
                return _facilityNoteRepository;
            }
        }

        public IStrokeCertificationRepository StrokeCertificationRepository
        {
            get
            {
                if (this._strokeCertificationRepository == null)
                {
                    this._strokeCertificationRepository = new StrokeCertificationRepository(context);
                }
                return _strokeCertificationRepository;
            }
        }

        public IPhysicianLicenseRepository PhysicianLicenseRepository
        {
            get
            {
                if (this._physicianLicenseRepository == null)
                {
                    this._physicianLicenseRepository = new PhysicianLicenseRepository(context);
                }
                return _physicianLicenseRepository;
            }
        }

        public ICaseAssignHistoryRepository CaseAssignHistoryRepository
        {
            get
            {
                if (this._caseAssignHistoryRepository == null)
                {
                    this._caseAssignHistoryRepository = new CaseAssignHistoryRepository(context);
                }
                return _caseAssignHistoryRepository;
            }
        }

        public IAppSettingRepository AppSettingRepository
        {
            get
            {
                if (this._appSettingRepository == null)
                {
                    this._appSettingRepository = new AppSettingRepository(context);
                }
                return _appSettingRepository;
            }
        }

        public IUserRepository UserRepository
        {
            get
            {
                if (this._userRepository == null)
                {
                    this._userRepository = new UserRepository(context);
                }
                return _userRepository;
            }
        }

        public IRoleRepository RoleRepository
        {
            get
            {
                if (this._roleRepository == null)
                {
                    this._roleRepository = new RoleRepository(context);
                }
                return _roleRepository;
            }
        }
        
        public IUserRoleRepository UserRoleRepository
        {
            get
            {
                if (this._userRoleRepository == null)
                {
                    this._userRoleRepository = new UserRoleRepository(context);
                }
                return _userRoleRepository;
            }
        }  

        public IUCLRepository UCLRepository
        {
            get
            {
                if (this._uclRepository == null)
                {
                    this._uclRepository = new UCLRepository(context);
                }
                return _uclRepository;
            }
        }

        public IUCL_UCDRepository UCL_UCDRepository
        {
            get
            {
                if (this.uCL_UCDRepository == null)
                {
                    this.uCL_UCDRepository = new UCL_UCDRepository(context);
                }
                return uCL_UCDRepository;
            }
        }

        public IScheduleRepository ScheduleRepository
        {
            get
            {
                if (this._scheduleRepository == null)
                {
                    this._scheduleRepository = new ScheduleRepository(context);
                }
                return _scheduleRepository;
            }
        }

        public Iphysician_status_snoozeRepository PhysicianStatusSnoozeRepository
        {
            get
            {
                if (this._physician_Status_SnoozeRepository == null)
                {
                    this._physician_Status_SnoozeRepository = new physician_status_snoozeRepository(context);
                }
                return _physician_Status_SnoozeRepository; 
            }
        }

        public INIHStrokeScaleQuestionRepository NIHStrokeScaleQuestionRepository
        {
            get
            {
                if (this._nIHStrokeScaleQuestionRepository == null)
                {
                    this._nIHStrokeScaleQuestionRepository = new NIHStrokeScaleQuestionRepository(context);
                }
                return _nIHStrokeScaleQuestionRepository;
            }
        }

        public INIHStrokeScaleRepository NIHStrokeScaleRepository
        {
            get
            {
                if (this._nIHStrokeScaleRepository == null)
                {
                    this._nIHStrokeScaleRepository = new NIHStrokeScaleRepository(context);
                }
                return _nIHStrokeScaleRepository;
            }
        }

        public ICaseCopyLogRepository CaseCopyLogRepository
        {
            get
            {
                if (this._caseCopyLogRepository == null)
                {
                    this._caseCopyLogRepository = new CaseCopyLogRepository(context);
                }
                return _caseCopyLogRepository;
            }
        }

        public IEAlertCaseTypesRepository EAlertCaseTypesRepository
        {
            get
            {
                if (this._eAlertCaseTypesRepository == null)
                {
                    this._eAlertCaseTypesRepository = new EAlertCaseTypesRepository(context);
                }
                return _eAlertCaseTypesRepository;
            }
        }
        public IEAlertFacilitiesRepository EAlertFacilitiesRepository
        {
            get
            {
                if (this._eAlertFacilitiesRepository == null)
                {
                    this._eAlertFacilitiesRepository = new EAlertFacilitiesRepository(context);
                }
                return _eAlertFacilitiesRepository;
            }
        }

        public INIHStrokeScaleAnswerRepository NIHStrokeScaleAnswerRepository
        {
            get
            {
                if (this._nIHStrokeScaleAnswerRepository == null)
                {
                    this._nIHStrokeScaleAnswerRepository = new NIHStrokeScaleAnswerRepository(context);
                }
                return _nIHStrokeScaleAnswerRepository;
            }
        }

        public ICaseGeneratedTemplateRepository CaseGeneratedTemplateRepository
        {
            get
            {
                if (this._caseGeneratedTemplateRepository == null)
                {
                    this._caseGeneratedTemplateRepository = new CaseGeneratedTemplateRepository(context);
                }
                return _caseGeneratedTemplateRepository;
            }
        }

        public ICaseTemplateStrokeTpaRepository CaseTemplateStrokeTpaRepository
        {
            get
            {
                if (this._caseTemplateStrokeTpaRepository == null)
                {
                    this._caseTemplateStrokeTpaRepository = new CaseTemplateStrokeTpaRepository(context);
                }
                return _caseTemplateStrokeTpaRepository;
            }
        }

        public ICaseTemplateStrokeNoTpaRepository CaseTemplateStrokeNoTpaRepository
        {
            get
            {
                if (this._caseTemplateStrokeNoTpaRepository == null)
                {
                    this._caseTemplateStrokeNoTpaRepository = new CaseTemplateStrokeNoTpaRepository(context);
                }
                return _caseTemplateStrokeNoTpaRepository;
            }
        }

        public ICaseTemplateStrokeNeuroTPARepository CaseTemplateStrokeNeuroTPARepository
        {
            get
            {
                if (this._caseTemplateStrokeNeuroTPARepository == null)
                {
                    this._caseTemplateStrokeNeuroTPARepository = new CaseTemplateStrokeNeuroTPARepository(context);
                }
                return _caseTemplateStrokeNeuroTPARepository;
            }
        }

        public ICaseTemplateTelestrokeNotpaRepository CaseTemplateTelestrokeNotpaRepository
        {
            get
            {
                if (this._caseTemplateTelestrokeNotpaRepository == null)
                {
                    this._caseTemplateTelestrokeNotpaRepository = new CaseTemplateTelestrokeNotpaRepository(context);
                }
                return _caseTemplateTelestrokeNotpaRepository;
            }
        }
        public IHospitalProtocols HospitalProtocols
        {
            get
            {
                if (this.__HospitalProtocols == null)
                {
                    this.__HospitalProtocols = new HospitalProtocols(context);
                }
                return __HospitalProtocols;
            }
        }
        public ICwhData cwhRepository
        {
            get
            {
                if (this._cwhRepository == null)
                {
                    this._cwhRepository = new CWHRepository(context);
                }
                return _cwhRepository;
            }
        }
        public IPhysicianStatusSnoozeOptionRepository PhysicianStatusSnoozeOptionRepository
        {
            get
            {
                if (this._physicianStatusSnoozeOptionRepository == null)
                {
                    this._physicianStatusSnoozeOptionRepository = new PhysicianStatusSnoozeOptionRepository(context);
                }
                return _physicianStatusSnoozeOptionRepository;
            }
        }


        public IAspNetUserDetailRepositorty AspNetUserDetailRepositorty
        {
            get
            {
                if (this._aspNetUserDetailRepositorty == null)
                {
                    this._aspNetUserDetailRepositorty = new AspNetUserDetailRepositorty(context);
                }
                return _aspNetUserDetailRepositorty;
            }
        }

        public IEntityChangeLogRepository EntityChangeLogRepository
        {
            get
            {
                if (this._entityChangeLogRepository == null)
                {
                    this._entityChangeLogRepository = new EntityChangeLogRepository(context);
                }
                return _entityChangeLogRepository; 
            }
        }
          #region Shiraz Code
        private IUserProfileRepository _UserProfileRepository; 
        public IUserProfileRepository UserProfileRepository 
        { 
            get
            {
                if (this._UserProfileRepository == null)
                {
                    this._UserProfileRepository = new UserProfileRepository(context);
                }
                return _UserProfileRepository;
            }
        }
        #endregion
        public IWeb2Campaign_LogRepository Web2Campaign_LogRepository
        {
            get
            {
                if (this._web2CampaignLogRepository == null)
                {
                    this._web2CampaignLogRepository = new Web2Campaign_LogRepository(context);
                }
                return _web2CampaignLogRepository;
            }
        }
        public IRapidsMailboxRepository RapidsMailboxRepository
        {
            get
            {
                if (this._rapidsMailboxRepository == null)
                {
                    this._rapidsMailboxRepository = new RapidsMailboxRepository(context);
                }
                return _rapidsMailboxRepository;
            }
        }
       
        public IPhysician_Case_TempRepository Physician_Case_TempRepository
        {
            get
            {
                if (this._physician_Case_TempRepository == null)
                {
                    this._physician_Case_TempRepository = new Physician_Case_TempRepository(context);
                }
                return _physician_Case_TempRepository;
            }
        }

        public IFacilityQuestionnairePreLiveRepository FacilityQuestionnairePreLiveRepository
        {
            get
            {
                if (this._facilityQuestionnairePreLiveRepository == null)
                {
                    this._facilityQuestionnairePreLiveRepository = new FacilityQuestionnairePreLiveRepository(context);
                }
                return _facilityQuestionnairePreLiveRepository;
            }
        }

        public IFacilityQuestionnaireContactDesignationRepository FacilityQuestionnaireContactDesignationRepository
        {
            get
            {
                if (this._facilityQuestionnaireContactDesignationRepository == null)
                {
                    this._facilityQuestionnaireContactDesignationRepository = new FacilityQuestionnaireContactDesignationRepository(context);
                }
                return _facilityQuestionnaireContactDesignationRepository;
            }
        }

        public IMockCaseRepository MockCaseRepository
        {
            get
            {
                if (this._mockCaseRepository == null)
                {
                    this._mockCaseRepository = new MockCaseRepository(context);
                }
                return _mockCaseRepository;
            }
        }

        public IFacilityQuestionnaireContactRespository FacilityQuestionnaireContactRespository
        {
            get
            {
                if (this._facilityQuestionnaireContactRespository == null)
                {
                    this._facilityQuestionnaireContactRespository = new FacilityQuestionnaireContactRespository(context);
                }
                return _facilityQuestionnaireContactRespository;
            }
        }

        //_physician_Case_TempRepository

        #region Husnain Code Block

        /// <summary>
        ///  rate part will be update in next updation --today date is 03 01 2020
        /// </summary>

        private IAlarmRepository _alarmRepository;
        public IAlarmRepository AlarmRepository
        {
            get
            {
                if (this._alarmRepository == null)
                {
                    this._alarmRepository = new AlarmSettingRepository(context);
                }
                return _alarmRepository;
            }
        }
        private IAlarmTuneRepository _alarmTuneRepository;
        public IAlarmTuneRepository AlarmTuneRepository
        {
            get
            {
                if (this._alarmTuneRepository == null)
                {
                    this._alarmTuneRepository = new AlarmTuneRepository(context);
                }
                return _alarmTuneRepository;
            }
        }

        // root cause
        private IRootCauseRepository _rootCauseRepository;
        public IRootCauseRepository RootCauseRepository
        {
            get
            {
                if (this._rootCauseRepository == null)
                {
                    this._rootCauseRepository = new RootCauseRepository(context);
                }
                return _rootCauseRepository;
            }
        }


        private IPhysicianRateRepository _physicianRateRepository;
        private IRateRepository _rateRepository;
        public IPhysicianRateRepository PhysicianRateRepository
        {
            get
            {
                if (this._physicianRateRepository == null)
                {
                    this._physicianRateRepository = new PhysicianRateRepository(context);
                }
                return _physicianRateRepository;
            }
        }
        public IRateRepository RateRepository
        {
            get
            {
                if (this._rateRepository == null)
                {
                    this._rateRepository = new RateRepository(context);
                }
                return _rateRepository;
            }
        }

        private IPhysicianHolidayRateRepository _physicianHolidayRateRepository;
        public IPhysicianHolidayRateRepository PhysicianHolidayRateRepository
        {
            get
            {
                if (this._physicianHolidayRateRepository == null)
                {
                    this._physicianHolidayRateRepository = new PhysicianHolidayRateRepository(context);
                }
                return _physicianHolidayRateRepository;
            }
        }
        private IPhysicianPercentageRateRepository _physicianPercentageRateRepository;
        public IPhysicianPercentageRateRepository PhysicianPercentageRateRepository
        {
            get
            {
                if (this._physicianPercentageRateRepository == null)
                {
                    this._physicianPercentageRateRepository = new PhysicianPercentageRateRepository(context);
                }
                return _physicianPercentageRateRepository;
            }
        }

        private ICaseReviewTemplateRepository _CaseReviewTemplateRepository;

        public ICaseReviewTemplateRepository CaseReviewTemplateRepository
        {
            get
            {
                if (this._CaseReviewTemplateRepository == null)
                {
                    this._CaseReviewTemplateRepository = new CaseReviewTemplateRepository(context);
                }
                return _CaseReviewTemplateRepository;
            }
        }

       

        private IPostAcuteCareRepository _postAcuteCareRepository;
        public IPostAcuteCareRepository PostAcuteCareRepository
        {
            get
            {
                if (this._postAcuteCareRepository == null)
                {
                    this._postAcuteCareRepository = new PostAcuteCareRepository(context);
                }
                return _postAcuteCareRepository;
            }
        }
        private IPremorbidCorrespondnceRepository _premorbidRepository;
        public IPremorbidCorrespondnceRepository premorbidRepository
        {
            get
            {
                if (this._premorbidRepository == null)
                {
                    this._premorbidRepository = new PremorbidCorrespondnceRepository(context);
                }
                return _premorbidRepository;
            }
        }

        #region PACTemplate
        private IPACGeneratedTemplateRepository _pacGeneratedTemplateRepository;
        public IPACGeneratedTemplateRepository PACGeneratedTemplateRepository
        {
            get
            {
                if (this._pacGeneratedTemplateRepository == null)
                {
                    this._pacGeneratedTemplateRepository = new PACGeneratedTemplateRepository(context);
                }
                return _pacGeneratedTemplateRepository;
            }
        }
        #endregion
        #region QualityGoals
        private IQualityGoalsRepository _qualityGoalsRepository;
        public IQualityGoalsRepository QualityGoalsRepository
        {
            get
            {
                if (this._qualityGoalsRepository == null)
                {
                    this._qualityGoalsRepository = new QualityGoalsRepository(context);
                }
                return _qualityGoalsRepository;
            }
        }
        private IGoalsDataRepository _goalsDataRepository;
        public IGoalsDataRepository GoalsDataRepository
        {
            get
            {
                if (this._goalsDataRepository == null)
                {
                    this._goalsDataRepository = new GoalsDataRepository(context);
                }
                return _goalsDataRepository;
            }
        }
        #endregion

      


        private ICaseTemplateStatConsultRepository _caseTemplateStatConsultRepository;
        public ICaseTemplateStatConsultRepository CaseTemplateStatConsultRepository
        {
            get
            {
                if (this._caseTemplateStatConsultRepository == null)
                {
                    this._caseTemplateStatConsultRepository = new CaseTemplateStatConsultRepository(context);
                }
                return _caseTemplateStatConsultRepository;
            }
        }

        #region Case Cancelled Types
        private ICasCancelledTypeRepository _casCancelledTypeRepository;
        public ICasCancelledTypeRepository CaseCancelledRepository
        {
            get
            {
                if (this._casCancelledTypeRepository == null)
                {
                    this._casCancelledTypeRepository = new CasCancelledTypeRepository(context);
                }
                return _casCancelledTypeRepository;
            }
        }
        #endregion


        #region Sleep Schedule
        private ISleepRepository _sleepRepository;
        public ISleepRepository SleepRepository
        {
            get
            {
                if (this._sleepRepository == null)
                {
                    this._sleepRepository = new SleepRepository(context);
                }
                return _sleepRepository;
            }
        }
        #endregion
        #region NH Schedule
        private INHRepository _nHRepository;
        public INHRepository NHRepository
        {
            get
            {
                if (this._nHRepository == null)
                {
                    this._nHRepository = new NHRepository(context);
                }
                return _nHRepository;
            }
        }
        #endregion

        #endregion

        #region Bilal Code Block

        private IUserVerificationRepoistory _UserVerificationRepository;
        public IUserVerificationRepoistory userVerificationRepoistory
        {
            get
            {
                if (this._UserVerificationRepository == null)
                {
                    this._UserVerificationRepository = new UserVerificationRepoistory(context);
                }
                return _UserVerificationRepository;
            }
        }

        private IDiagnosisCodesRepoistory _DiagnosisCodesRepository;
        public IDiagnosisCodesRepoistory DiagnosisCodesRepoistory
        {
            get
            {
                if (this._DiagnosisCodesRepository == null)
                {
                    this._DiagnosisCodesRepository = new DiagnosisCodesRepoistory(context);
                }
                return _DiagnosisCodesRepository;
            }
        }

        private IIcd10CodesCalRepository _Icd10CodesCalRepository;
        public IIcd10CodesCalRepository Icd10CodesCalRepository
        {
            get
            {
                if (this._Icd10CodesCalRepository == null)
                {
                    this._Icd10CodesCalRepository = new Icd10CodesCalRepository(context);
                }
                return _Icd10CodesCalRepository;
            }
        }
        #endregion

        #region Firebase

        private ITokenRepository _tokenRepository;
        public ITokenRepository TokenRepository
        {
            get
            {
                if (this._tokenRepository == null)
                {
                    this._tokenRepository = new TokenRepository(context);
                }
                return _tokenRepository;
            }
        }

        private IFireBaseUserMailRepository _fireBaseUserMailRepository;
        public IFireBaseUserMailRepository FireBaseUserMailRepository
        {
            get
            {
                if (this._fireBaseUserMailRepository == null)
                {
                    this._fireBaseUserMailRepository = new FireBaseUserMailRepository(context);
                }
                return _fireBaseUserMailRepository;
            }
        }
        #endregion

        #region Case Rejection Reason
        private ICaseRejectRepository _casRejectRepository;
        public ICaseRejectRepository CaseRejectRepository
        {
            get
            {
                if (this._casRejectRepository == null)
                {
                    this._casRejectRepository = new CaseRejectRepository(context);
                }
                return _casRejectRepository;
            }
        }
        #endregion

        #endregion ----- Repositories -----

        #region ----- Save -----
        public async Task SaveAsync()
        {
            //await context.SaveChangesAsync();
            context.SaveChanges();
        }
        public void Save()
        {
            context.SaveChanges();
        }
        #endregion

        #region ----- IDisposable -----

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _transactionScope?.Dispose();
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
