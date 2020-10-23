using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;

namespace TeleSpecialists.BLL.Process.MDStaff
{
    public class MDStaffImport : MDStaffBase
    {
        private string _createdBy = Guid.Empty.ToString();
        private string _createdByName = "Data Import";
        private readonly DateTime _createdDate = DateTime.Now.ToEST();
        private string _requestId = "";
        private string _currentPhysician = "";

        private List<AspNetUser> _listOfPhysicians = new List<AspNetUser>();
        private List<AspNetUser_Detail> _listOfPhysicianDetails = new List<AspNetUser_Detail>();

        private List<physician_license> _listOfCredentials = new List<physician_license>();
        private List<facility_physician> _listOfAssociation = new List<facility_physician>();
        private List<facility> _listOfDBFacilities = new List<facility>();

        #region ----- Service Things -----

        /*
        private Thread _thread;
        private readonly ManualResetEvent _shutdownEvent;



        public MDStaffProcessor()
        {
            _shutdownEvent = new ManualResetEvent(false);
        }

        public void StartService()
        {
            _thread = new Thread(DoWork);
            _thread.IsBackground = true;
            _thread.Start();
        }

        public void StopService()
        {
            _shutdownEvent.Set();

            EmailHelper.ServiceErrorEmail();
            _logger.AddLogEntry(_serviceName, "ALERT", "Stopped", "");

            if (!_thread.Join(1))
            {
                // give the thread 3 seconds to stop
                _thread.Abort();
            }
        }

        private void DoWork()
        {
            int sleepInterval = 5;

            while (!_shutdownEvent.WaitOne(0))
            {
                try
                {
                    // get service sleep interval
                    int.TryParse(System.Configuration.ConfigurationManager.AppSettings.Get("SleepInterval"), out sleepInterval);

                    // create log
                    _logger.AddLogEntry(_serviceName, "INPROGRESS", _serviceName + " Started", "");

                    string requestId = Guid.NewGuid().ToString();

                    // start processing
                    this.GetAll(requestId, Guid.Empty.ToString(), "Data Import Service").Wait();
                    this.DataSyncComplete(requestId).Wait();
                }
                catch (Exception exception)
                {
                    _logger.AddLogEntry(_serviceName, "ERROR", exception, "");
                }
                finally
                {
                    _logger.AddLogEntry(_serviceName, "COMPLETED", "", "");
                }

                _logger.AddLogEntry(_serviceName, "INFO", "Sleeping for " + sleepInterval.ToString() + " minutes.", "");
                Thread.Sleep(new TimeSpan(0, sleepInterval, 0));
            }

            // if house keeper exit the main thread
            _logger.AddLogEntry(_serviceName, "ALERT", "Exiting Service main thread", "DoWork");
        }

        */
        #endregion  

        #region ----- Get Information from MD Staff API -----

        private async Task<string> GetAllProviderIDs()
        {
            // providers url
            string url = "[base_url]/api/[instance]/providers";

            return await Get(url);
        }
        private async Task<string> GetProvider(string id)
        {
            // providers url
            string url = string.Format("[base_url]/api/[instance]/providers/{0}", id);

            return await Get(url);
        }

        private async Task<string> GetProviderAppointment(string id)
        {
            string url = string.Format("[base_url]/api/[instance]/providers/{0}/appointment", id); // API Description GET api/{ instance}/ providers /{ providerID}/ appointment
            return await Get(url);
        }
        private async Task<string> GetCredential(string id)
        {
            // providers url
            string url = string.Format("[base_url]/api/[instance]/providers/{0}/credential", id);

            return await Get(url);
        }
        private async Task<string> GetHospital(string id)
        {
            // providers url
            // https://api.mdstaff.com/webapi/api/telespecialists/providers/ece6fc69-2c8a-4133-b5ed-5e52cad3ec1e/hospital

            string url = string.Format("[base_url]/api/[instance]/providers/{0}/hospital", id);

            return await Get(url);
        }



        #endregion

        #region ----- Parse Json ----- 

        private async Task<AspNetUser> ParseProvider(string providerID)
        {

            await AddLogEntry("INPROGRESS", _requestId, "Getting provider appointment details", this._createdByName, providerID);
            string providerAppointmentJson = await this.GetProviderAppointment(providerID);
            if (!string.IsNullOrEmpty(providerAppointmentJson))
            {
                JArray appointmentJsonObjectArray = JArray.Parse(providerAppointmentJson);
                if (appointmentJsonObjectArray != null && appointmentJsonObjectArray.Count() > 0)
                {
                    dynamic appointmentJsonObject = appointmentJsonObjectArray.First();
                    string providerEmail = (string)appointmentJsonObject.ProviderTelespecialistEmail;
                    string providerContractDate = (string)appointmentJsonObject.ContractDate;


                    if (!string.IsNullOrEmpty(providerEmail) && !string.IsNullOrEmpty(providerContractDate))
                    {

                        await AddLogEntry("INPROGRESS", _requestId, "Getting provider details", this._createdByName, providerID);

                        string providerJson = await this.GetProvider(providerID);

                        if (!string.IsNullOrEmpty(providerJson))
                        {
                            // convert to object
                            dynamic jsonObject = JsonConvert.DeserializeObject(providerJson);

                            await AddLogEntry("INPROGRESS", _requestId, "Parsing provider details", this._createdByName, providerID);

                            // if converted to object
                            if (jsonObject != null)
                            {
                                // string username = (string)jsonObject.Email;

                                if (providerEmail.Contains(";")) providerEmail = (providerEmail.Split(';')[0]).Trim();

                                var physician = new AspNetUser
                                {
                                    Id = Convert.ToString(jsonObject.ID),
                                    UserName = providerEmail,
                                    Email = providerEmail,
                                    FirstName = (string)jsonObject.FirstName,
                                    LastName = (string)jsonObject.LastName,
                                    Gender = (string)jsonObject.Gender,
                                    NPINumber = ((string)jsonObject.NPI).Trim(),
                                    MobilePhone = (string)jsonObject.CellPhone,
                                    AddressBlock = (string)jsonObject.AddressBlock,
                                    IsActive = true,
                                    ContractDate = providerContractDate.ToDateTime(),
                                    CreatedBy = this._createdBy,
                                    CreatedByName = this._createdByName,
                                    CreatedDate = this._createdDate,
                                };

                                GenerateInitial(physician);
                                string photo = (string)jsonObject.Photo;

                                if (!string.IsNullOrEmpty(photo))
                                {
                                    _listOfPhysicianDetails.Add(new AspNetUser_Detail
                                    {
                                        Id = physician.Id,
                                        PhotoBase64 = jsonObject.Photo,
                                        CreatedBy = this._createdBy,
                                        CreatedByName = this._createdByName,
                                        CreatedDate = this._createdDate,
                                    });

                                }

                                _listOfPhysicians.Add(physician);

                                _currentPhysician = string.Format("{0} {1}", physician.FirstName, physician.LastName);
                                return physician;
                            }
                        }
                    }
                }

            }

            return null;
        }
        private async Task ParseCredentials(string providerID)
        {
            await AddLogEntry("INPROGRESS", _requestId, "Getting provider credentials - " + _currentPhysician, this._createdByName, providerID);

            // get provider details
            string resultJson = await this.GetCredential(providerID);

            if (!string.IsNullOrEmpty(resultJson))
            {
                await AddLogEntry("INPROGRESS", _requestId, "Parsing provider credentials - " + _currentPhysician, this._createdByName, providerID);

                // convert to object
                JArray jsonObject = JArray.Parse(resultJson);

                // if converted to object
                if (jsonObject != null && jsonObject.Count() > 0)
                {
                    foreach (JObject item in jsonObject.Children<JObject>())
                    {
                        if (string.IsNullOrEmpty(Convert.ToString(item.Property("LicenseNumber").Value)))
                            continue;

                        if ((Convert.ToString(item.Property("LicenseNumber").Value)).ToLower() == "pending")
                            continue;

                        if (string.IsNullOrEmpty(Convert.ToString(item.Property("Issued").Value)))
                            continue;

                        var license = new physician_license
                        {
                            phl_key = (Guid)item.Property("CredentialID").Value,
                            phl_user_key = providerID,
                            phl_license_type = (Guid)item.Property("LicenseTypeID").Value,
                            phl_license_number = (string)item.Property("LicenseNumber").Value,
                            phl_issued_date = (DateTime)item.Property("Issued").Value,
                            phl_expired_date = (DateTime?)item.Property("Expired").Value,
                            phl_is_in_use = (bool)item.Property("InUse").Value,
                            phl_is_active = true,
                            phl_app_started= (DateTime?)item.Property("ApplicationStarted").Value,
                            phl_app_submitted_to_board= (DateTime?)item.Property("ApplicationSubmittedToBoard").Value,
                            phl_date_assigned= (DateTime?)item.Property("DateAssigned").Value,
                            phl_app_sent_to_provider= (DateTime?)item.Property("ApplicationSentToProvider").Value,

                            phl_created_date = this._createdDate,
                            phl_created_by = this._createdBy,
                            phl_created_by_name = this._createdByName
                        };
                        if (item.Property("ProcessingAssignedTo") != null && !string.IsNullOrEmpty(Convert.ToString(item.Property("ProcessingAssignedTo").Value)))
                        {
                            var ProcessingAssignedToObj= ((JObject)item.Property("ProcessingAssignedToObj").Value);
                            if (ProcessingAssignedToObj != null)
                            {
                                license.phl_assigned_to_id= (Guid)ProcessingAssignedToObj.Property("ID").Value;
                                license.phl_assigned_to_name= (string)ProcessingAssignedToObj.Property("Description").Value;
                            }

                        }

                            if (item.Property("LicensureBoard") != null && !string.IsNullOrEmpty(Convert.ToString(item.Property("LicensureBoard").Value)))
                        {
                            var licensureBoard = ((JObject)item.Property("LicensureBoard").Value);
                            if (licensureBoard != null)
                            {
                                if (licensureBoard.Property("Uid") != null)
                                    license.phl_licensure_board_id = (string)licensureBoard.Property("Uid").Value;

                                if (licensureBoard.Property("Name") != null)
                                    license.phl_licensure_board_name = (string)licensureBoard.Property("Name").Value;
                            }
                        }

                        // map state to local state
                        string strMDState = (string)item.Property("State").Value;
                        license.phl_license_state = AllStates.Where(x => x.ucd_title == strMDState).FirstOrDefault()?.ucd_key;

                        _listOfCredentials.Add(license);
                    }
                }
            }
        }
        private async Task ParseHospitals(string providerID)
        {
            await AddLogEntry("INPROGRESS", _requestId, "Getting provider hospitals - " + _currentPhysician, this._createdByName, providerID);

            // get provider details
            string resultJson = await this.GetHospital(providerID);

            if (!string.IsNullOrEmpty(resultJson))
            {
                await AddLogEntry("INPROGRESS", _requestId, "Parsing provider hospitals - " + _currentPhysician, this._createdByName, providerID);

                // convert to object
                JArray jsonObject = JArray.Parse(resultJson);

                // if converted to object
                if (jsonObject != null && jsonObject.Count() > 0)
                {
                    foreach (JObject item in jsonObject.Children<JObject>())
                    {
                        var referenceSource = ((JObject)item.Property("ReferenceSource").Value);
                        if (referenceSource != null)
                        {
                            string facSourceName = Convert.ToString(referenceSource.Property("Name").Value).Trim();
                            string facSourceID = Convert.ToString(referenceSource.Property("ReferenceSourceID").Value);


                            // match MD Staff facility with local db
                            var tempFacilities = this._listOfDBFacilities.Where(x => (x.fac_md_staff_source_name == null ? "" : x.fac_md_staff_source_name).Trim().ToLower() == facSourceName.ToLower()).ToList();

                            // if matched, check|create mapping
                            if (tempFacilities != null && tempFacilities.Count() > 0)
                            {
                                if (_listOfAssociation == null) _listOfAssociation = new List<facility_physician>();

                                foreach (var tempFacility in tempFacilities)
                                {
                                    #region ----- Facility Physician Association -----

                                    var association = new facility_physician
                                    {
                                        // fap_key // autonumber 
                                        fap_fac_key = tempFacility.fac_key, //(Guid)item.Property("ReferenceSourceID").Value,
                                        fap_user_key = providerID,
                                        fap_start_date = (DateTime?)item.Property("StartDate").Value,
                                        fap_end_date = (DateTime?)item.Property("EndDate").Value,

                                        fap_onboarding_complete_provider_active_date = (DateTime?)item.Property("OnboardingCompleteProviderActiveDate").Value,

                                        fap_is_on_boarded = false,
                                        fap_is_active = true,
                                        fap_created_date = this._createdDate,
                                        fap_created_by = this._createdBy,
                                        fap_created_by_name = this._createdByName,
                                        fap_date_assigned= (DateTime?)item.Property("DateAssigned").Value,
                                        fap_initial_app_received= (DateTime?)item.Property("ReapptReceivedDate").Value,
                                        fap_app_started= (DateTime?)item.Property("ApplicationStarted").Value,
                                        fap_app_submitted_to_hospital= (DateTime?)item.Property("ApplicationSubmittedToHospital").Value,
                                        fap_vcaa_date= (DateTime?)item.Property("VCAADate").Value,
                                        fap_anticipated_start= (DateTime?)item.Property("AnticipatedStartDate").Value,
                                        fap_preapp_request_date= (DateTime?)item.Property("PreApplicationRequested").Value,
                                        fap_preapp_submitted_date= (DateTime?)item.Property("PreApplicationSubmitted").Value,
                                        fap_preapp_receive_date= (DateTime?)item.Property("PreApplicationReceived").Value,
                                        fap_app_sent_to_dr= (DateTime?)item.Property("ApplicationSentToProvider").Value,
                                        fap_Credentials_confirmed_date= (DateTime?)item.Property("CredentialingConfirmedDate").Value,
                                    };
                                    if (item.Property("MandPAssignedTo") != null && !string.IsNullOrEmpty(Convert.ToString(item.Property("MandPAssignedTo").Value)))
                                    {
                                        var MandPAssignedToObj = ((JObject)item.Property("MandPAssignedToObj").Value);
                                        if (MandPAssignedToObj != null)
                                        {
                                            association.fap_credential_specialist = (string)MandPAssignedToObj.Property("Description").Value;
                                        }

                                    }
                                    if (item.Property("AssignedTo") != null && !string.IsNullOrEmpty(Convert.ToString(item.Property("AssignedTo").Value)))
                                    {
                                        var AssignedToObj = ((JObject)item.Property("AssignedToObj").Value);
                                        if (AssignedToObj != null)
                                        {
                                            association.fap_app_specialist = (string)AssignedToObj.Property("Description").Value;
                                        }

                                    }

                                    if (!_listOfAssociation.Any(x => x.fap_fac_key == association.fap_fac_key && x.fap_user_key == association.fap_user_key))
                                        _listOfAssociation.Add(association);

                                    #endregion
                                }
                            }

                            facSourceName = facSourceName.Replace("'", "''").Trim();

                            // update facility reference source ID  where source name is facSourceName
                            string query = string.Format("Update facility set  fac_md_staff_reference_source_id = '{0}' where fac_md_staff_source_name = '{1}'", facSourceID, facSourceName);

                            this._unitOfWork.ExecuteSqlCommand(query);
                        }

                        #region ----- Old Logic -----

                        /*
                        var hospital = new facility
                        {
                            fac_key = (Guid)item.Property("ReferenceSourceID").Value,
                            fac_timezone = "Eastern Standard Time",
                            fac_timezone_offset_hours = -5,
                            //fac_description = (string)item.Property("Comments").Value,

                            //fac_fct_key = item.Property("ReferenceType").Value, // facility type
                            //fac_cst_key = item.Property("fac_cst_key").Value, // Stroke Certification
                            //fac_sct_key = item.Property("fac_sct_key").Value, // Computer System

                            fac_is_active = true,
                            fac_created_by = _createdBy,
                            fac_created_by_name = this._createdByName,
                            fac_created_date = _createdDate,
                        };


                        var referenceSource = ((JObject)item.Property("ReferenceSource").Value);
                        if (referenceSource != null)
                        {
                            hospital.fac_name = (string)referenceSource.Property("Name").Value;
                            hospital.fac_address_line1 = (string)referenceSource.Property("Address").Value;
                            hospital.fac_address_line2 = (string)referenceSource.Property("Address2").Value;
                            hospital.fac_city = (string)referenceSource.Property("City").Value;
                            hospital.fac_zip = (string)referenceSource.Property("Zip").Value;
                            hospital.fac_md_staff_source_name = hospital.fac_name;

                            // map state to local state
                            string strMDState = (string)referenceSource.Property("State").Value;
                            hospital.fac_stt_key = AllStates.Where(x => x.stt_code == strMDState).FirstOrDefault()?.stt_key;

                            // map type to local state
                            string strMDReferenceType = (string)referenceSource.Property("ReferenceType").Value;
                            hospital.fac_fct_key = AllFacilityTypes.Where(x => x.fct_name == strMDReferenceType).FirstOrDefault()?.fct_key;

                            await AddLogEntry("INPROGRESS", _requestId, "Getting provider hospitals contact/facility info - " + _currentPhysician + " > " + hospital.fac_name, this._createdByName, providerID);

                            #region ----- Contact Info -----

                            bool hasContact = false;

                            if (item.Property("Contact1").Value.HasValues)
                            {
                                var contact = new contact
                                {
                                    cnt_fac_key = hospital.fac_key,
                                    cnt_first_name = (string)referenceSource.Property("Contact1").Value,
                                    //cnt_role = (string)referenceSource.Property("ReferenceType").Value,
                                    cnt_primary_phone = (string)referenceSource.Property("Telephone").Value,
                                    cnt_mobile_phone = (string)referenceSource.Property("Fax").Value,
                                    cnt_email = (string)referenceSource.Property("Email").Value,
                                    cnt_is_active = true,
                                    cnt_created_date = _createdDate,
                                    cnt_created_by = _createdBy,
                                    cnt_created_by_name = _createdByName,
                                };

                                // init facility contacts
                                if (hospital.contacts == null) hospital.contacts = new List<contact>();

                                // add contact into list
                                hospital.contacts.Add(contact);

                                hasContact = true;
                            }

                            if (item.Property("Contact2").Value.HasValues)
                            {
                                var contact = new contact
                                {
                                    cnt_fac_key = hospital.fac_key,
                                    cnt_first_name = (string)referenceSource.Property("Contact2").Value,
                                    //cnt_role = (string)referenceSource.Property("ReferenceType").Value,
                                    cnt_is_active = true,
                                    cnt_created_date = _createdDate,
                                    cnt_created_by = _createdBy,
                                    cnt_created_by_name = _createdByName,
                                };

                                // init facility contacts
                                if (hospital.contacts == null) hospital.contacts = new List<contact>();

                                // add contact into list
                                hospital.contacts.Add(contact);

                                hasContact = true;
                            }

                            if (item.Property("Contact3").Value.HasValues)
                            {
                                var contact = new contact
                                {
                                    cnt_fac_key = hospital.fac_key,
                                    cnt_first_name = (string)referenceSource.Property("Contact3").Value,
                                    //cnt_role = (string)referenceSource.Property("ReferenceType").Value,
                                    cnt_is_active = true,
                                    cnt_created_date = _createdDate,
                                    cnt_created_by = _createdBy,
                                    cnt_created_by_name = _createdByName,
                                };

                                // init facility contacts
                                if (hospital.contacts == null) hospital.contacts = new List<contact>();

                                // add contact into list
                                hospital.contacts.Add(contact);

                                hasContact = true;
                            }

                            if (!hasContact && referenceSource.Property("Manager").Value.HasValues)
                            {
                                var contact = new contact
                                {
                                    cnt_fac_key = hospital.fac_key,
                                    cnt_first_name = (string)referenceSource.Property("Manager").Value,
                                    //cnt_role = (string)referenceSource.Property("ReferenceType").Value,
                                    cnt_primary_phone = (string)referenceSource.Property("Telephone").Value,
                                    cnt_mobile_phone = (string)referenceSource.Property("Fax").Value,
                                    cnt_email = (string)referenceSource.Property("Email").Value,
                                    cnt_is_active = true,
                                    cnt_created_date = _createdDate,
                                    cnt_created_by = _createdBy,
                                    cnt_created_by_name = _createdByName,
                                };

                                // init facility contacts
                                if (hospital.contacts == null) hospital.contacts = new List<contact>();

                                // add contact into list
                                hospital.contacts.Add(contact);

                                hasContact = true;
                            }

                            #endregion

                            #region ----- Facility Physician Association -----

                            var association = new facility_physician
                            {
                                // fap_key // autonumber 
                                fap_fac_key = hospital.fac_key,
                                fap_user_key = providerID,
                                fap_start_date = (DateTime?)item.Property("StartDate").Value,
                                fap_end_date = (DateTime?)item.Property("EndDate").Value,

                                fap_is_on_boarded = false,
                                fap_is_active = true,
                                fap_created_date = this._createdDate,
                                fap_created_by = this._createdBy,
                                fap_created_by_name = this._createdByName,
                            };

                            if (_listOfAssociation == null) _listOfAssociation = new List<facility_physician>();


                            if (!_listOfAssociation.Any(x => x.fap_fac_key == association.fap_fac_key && x.fap_user_key == association.fap_user_key))
                                _listOfAssociation.Add(association);

                            //hospital.facility_physician = new List<facility_physician>() { association };

                            #endregion

                            // save in database | if not already in the list
                            if (!_listOfFacilities.Any(x => x.fac_key == hospital.fac_key))
                                _listOfFacilities.Add(hospital);
                        }

                        */

                        #endregion
                    }
                }
            }
        }
        private void GenerateInitial(AspNetUser physician)
        {
            string tempInitial = string.Empty;

            // if first name exists, then take first letter
            if (!string.IsNullOrEmpty(physician.FirstName))
                tempInitial += physician.FirstName.Substring(0, 1);

            // if last name exists, then take first letter
            if (!string.IsNullOrEmpty(physician.LastName))
                tempInitial += physician.LastName.Substring(0, 1);

            string testInitial = tempInitial;

            var existingInitials = _unitOfWork.ApplicationUsers.Where(x => x.UserInitial == testInitial).FirstOrDefault();

            int initialCount = 2;

        initialAgain:

            if (existingInitials != null && existingInitials.UserInitial == testInitial)
            {
                testInitial = tempInitial + initialCount.ToString();

                initialCount++;

                existingInitials = _unitOfWork.ApplicationUsers.Where(x => x.UserInitial == testInitial).FirstOrDefault();

                goto initialAgain;
            }

            physician.UserInitial = testInitial;
        }

        #endregion


        private async Task<AspNetUser> GetMDStaffData(string providerID)
        {
            var physician = await ParseProvider(providerID);
            if (physician != null)
            {

                await ParseCredentials(providerID);

                await ParseHospitals(providerID);
            }
            return physician;
        }

        private async Task SaveMDStaffData(string providerID)
        {
            await AddLogEntry("INPROGRESS", _requestId, "Saving provider data - " + _currentPhysician, this._createdByName, providerID);

            _unitOfWork.SetChangeTrackiing(false);

            #region ----- Save Physicians -----

            // save new records only
            var newUsers = _listOfPhysicians.Where(x => !_unitOfWork.ApplicationUsers.Any(y => y.Id == x.Id))
                                            .Select(x => x)
                                            .ToList();
            // assign physician role
            newUsers.ForEach(x => x.AspNetUserRoles = new List<AspNetUserRole> { new AspNetUserRole { UserRoleId = Guid.NewGuid().ToString(), RoleId = _appSetting.aps_physician_role_id, UserId = x.Id } });


            _unitOfWork.UserRepository.InsertRange(newUsers);

            var existingUsers = _listOfPhysicians.Where(x => !newUsers.Any(y => y.Id == x.Id))
                                                    .Select(x => x)
                                                    .ToList();

            // saving/syncing existing physicians
            _unitOfWork.UserRepository.UpdateRange(UpdatePhysicians(existingUsers));

            #endregion

            #region ----- Saving Photo -----

            var newDetails = _listOfPhysicianDetails.Where(x => !_unitOfWork.AspNetUserDetailRepositorty.Query().Any(y => y.Id == x.Id))
                                            .Select(x => x)
                                            .ToList();

            _unitOfWork.AspNetUserDetailRepositorty.InsertRange(newDetails);


            var existingDetails = _listOfPhysicianDetails.Where(x => !newDetails.Any(y => y.Id == x.Id))
                                                  .Select(x => x)
                                                  .ToList();

            _unitOfWork.AspNetUserDetailRepositorty.UpdateRange(UpdatePhysicianDetails(existingDetails));

            #endregion

            #region ----- Save License -----

            var newLicenses = (from c in _listOfCredentials
                               join dc in _unitOfWork.PhysicianLicenseRepository.Query()//.Where(x => x.phl_user_key == providerID).ToList()
                               on c.phl_key equals dc.phl_key

                               into tempList
                               from dd in tempList.DefaultIfEmpty()
                               where dd == null
                               select c)
                              .ToList();

            // save credentials
            _unitOfWork.PhysicianLicenseRepository.InsertRange(newLicenses);

            // update existing license 
            var existingLicense = _listOfCredentials.Where(x => !newLicenses.Any(y => y.phl_key == x.phl_key))
                                                    .Select(x => x)
                                                    .ToList();

            _unitOfWork.PhysicianLicenseRepository.UpdateRange(UpdateLicense(existingLicense));

            #endregion


            // save evrything in database
            _unitOfWork.BeginTransaction();

            await _unitOfWork.SaveAsync();

            #region ----- Save Credentials -----

            // save new physician+facility associations
            /*
            var newAssociations = _listOfAssociation.Where(x => !_unitOfWork.FacilityPhysicianRepository.Query().Any(y => y.fap_fac_key == x.fap_fac_key && y.fap_user_key == providerID))
                                                    .Select(x => x)
                                                    .ToList();
            */

            var newAssociations = (from a in _listOfAssociation
                                   join b in _unitOfWork.FacilityPhysicianRepository.Query()//.Where(x => x.phl_user_key == providerID).ToList()
                                   on new { a.fap_fac_key, provider = providerID } equals new { b.fap_fac_key, provider = b.fap_user_key }

                                   into tempList
                                   from c in tempList.DefaultIfEmpty()
                                   where c == null
                                   select a)
                                   .ToList();

            _unitOfWork.FacilityPhysicianRepository.InsertRange(newAssociations);

            await _unitOfWork.SaveAsync();


            // update existing license 
            var existingAssociations = _listOfAssociation.Where(x => !newAssociations.Any(y => y.fap_fac_key == x.fap_fac_key && y.fap_user_key == x.fap_user_key))
                                                         .Select(x => x)
                                                         .ToList();

            _unitOfWork.FacilityPhysicianRepository.UpdateRange(UpdateAssociations(existingAssociations));

            await _unitOfWork.SaveAsync();

            #endregion

            _unitOfWork.Commit();
            _unitOfWork.SetChangeTrackiing(true);
        }

        private async Task MarkUserInactive(string providerID)
        {
            var physician = _unitOfWork.UserRepository.Query().FirstOrDefault(m => m.Id == providerID);
            if (physician != null)
            {
                physician.IsActive = false;
                physician.ModifiedDate = _createdDate;
                await _unitOfWork.SaveAsync();
                _unitOfWork.Commit();
            }
        }

        private List<AspNetUser> UpdatePhysicians(List<AspNetUser> existingEntries)
        {
            List<AspNetUser> updatedPhysicians = new List<AspNetUser>();
            var distinctUseIds = existingEntries.Select(m => m.Id).ToList();
            var allPhysicians = _unitOfWork.UserRepository.Query().Where(x => distinctUseIds.Contains(x.Id));

            foreach (var updatedEntry in existingEntries)
            {
                var dbEntry = allPhysicians.Where(x => x.Id == updatedEntry.Id).FirstOrDefault();

                if (dbEntry != null)
                {
                    dbEntry.ModifiedBy = _createdBy;
                    dbEntry.ModifiedByName = _createdByName;
                    dbEntry.ModifiedDate = _createdDate;
                    dbEntry.UserName = updatedEntry.Email;
                    dbEntry.ContractDate = updatedEntry.ContractDate;
                    dbEntry.FirstName = updatedEntry.FirstName;
                    dbEntry.LastName = updatedEntry.LastName;
                    dbEntry.Email = updatedEntry.Email;
                    dbEntry.MobilePhone = updatedEntry.MobilePhone;
                    dbEntry.PhoneNumber = updatedEntry.PhoneNumber;
                    dbEntry.NPINumber = updatedEntry.NPINumber;

                    // dbEntry.AspNetUser_Detail = updatedEntry.AspNetUser_Detail;
                    updatedPhysicians.Add(dbEntry);
                }
            }
            return updatedPhysicians;
        }

        private List<AspNetUser_Detail> UpdatePhysicianDetails(List<AspNetUser_Detail> existingDetails)
        {
            List<AspNetUser_Detail> updatedPhysicians = new List<AspNetUser_Detail>();
            var distinctUseIds = existingDetails.Select(m => m.Id).ToList();
            var allPhysicians = _unitOfWork.AspNetUserDetailRepositorty.Query().Where(x => distinctUseIds.Contains(x.Id));

            foreach (var updatedEntry in existingDetails)
            {
                var dbEntry = allPhysicians.Where(x => x.Id == updatedEntry.Id).FirstOrDefault();

                if (dbEntry != null)
                {
                    dbEntry.ModifiedBy = _createdBy;
                    dbEntry.ModifiedByName = _createdByName;
                    dbEntry.ModifiedDate = _createdDate;
                    dbEntry.PhotoBase64 = updatedEntry.PhotoBase64;

                    // dbEntry.AspNetUser_Detail = updatedEntry.AspNetUser_Detail;
                    updatedPhysicians.Add(dbEntry);
                }
            }
            return updatedPhysicians;
        }

        private List<physician_license> UpdateLicense(List<physician_license> existingEntries)
        {
            List<physician_license> updatedPhyLicenses = new List<physician_license>();
            var distinctUseIds = existingEntries.Select(m => m.phl_user_key).ToList();
            var allPhyLiscences = _unitOfWork.PhysicianLicenseRepository.Query().Where(x => distinctUseIds.Contains(x.phl_user_key)).ToList();

            foreach (var updatedEntry in existingEntries)
            {
                var dbEntry = allPhyLiscences.Where(x => x.phl_key == updatedEntry.phl_key).FirstOrDefault();
                if (dbEntry != null)
                {
                    dbEntry.phl_license_type = updatedEntry.phl_license_type;
                    dbEntry.phl_license_state = updatedEntry.phl_license_state;
                    dbEntry.phl_license_number = updatedEntry.phl_license_number;
                    dbEntry.phl_issued_date = updatedEntry.phl_issued_date;
                    dbEntry.phl_expired_date = updatedEntry.phl_expired_date;
                    dbEntry.phl_licensure_board_id = updatedEntry.phl_licensure_board_id;
                    dbEntry.phl_licensure_board_name = updatedEntry.phl_licensure_board_name;
                    dbEntry.phl_is_in_use = updatedEntry.phl_is_in_use;
                    dbEntry.phl_modified_by = _createdBy;
                    dbEntry.phl_modified_by_name = _createdByName;
                    dbEntry.phl_modified_date = _createdDate;
                    dbEntry.phl_app_started = updatedEntry.phl_app_started;
                    dbEntry.phl_app_submitted_to_board = updatedEntry.phl_app_submitted_to_board;
                    dbEntry.phl_date_assigned = updatedEntry.phl_date_assigned;
                    dbEntry.phl_app_sent_to_provider = updatedEntry.phl_app_sent_to_provider;
                    dbEntry.phl_assigned_to_id = updatedEntry.phl_assigned_to_id;
                    dbEntry.phl_assigned_to_name = updatedEntry.phl_assigned_to_name;

                updatedPhyLicenses.Add(dbEntry);
                }
            }
            return updatedPhyLicenses;
        }

        private List<facility_physician> UpdateAssociations(List<facility_physician> existingEntries)
        {
            List<facility_physician> updatedPhyFacilities = new List<facility_physician>();
            var distinctUseIds = existingEntries.Select(m => m.fap_user_key).ToList();
            var allPhyFacilities = _unitOfWork.FacilityPhysicianRepository.Query().Where(x => distinctUseIds.Contains(x.fap_user_key)).ToList();

            foreach (var updatedEntry in existingEntries)
            {
                var dbEntry = allPhyFacilities.Where(x => x.fap_fac_key == updatedEntry.fap_fac_key).FirstOrDefault();

                if (dbEntry != null)
                {
                    dbEntry.fap_start_date = updatedEntry.fap_start_date;
                    dbEntry.fap_end_date = updatedEntry.fap_end_date;
                    dbEntry.fap_onboarding_complete_provider_active_date = updatedEntry.fap_onboarding_complete_provider_active_date;

                    dbEntry.fap_modified_by = _createdBy;
                    dbEntry.fap_modified_by_name = _createdByName;
                    dbEntry.fap_modified_date = _createdDate;
                    dbEntry.fap_date_assigned = updatedEntry.fap_date_assigned;
                    dbEntry.fap_initial_app_received = updatedEntry.fap_initial_app_received;
                    dbEntry.fap_app_started = updatedEntry.fap_app_started;
                    dbEntry.fap_app_submitted_to_hospital = updatedEntry.fap_app_submitted_to_hospital;
                    dbEntry.fap_vcaa_date = updatedEntry.fap_vcaa_date;
                    dbEntry.fap_anticipated_start = updatedEntry.fap_anticipated_start;
                    dbEntry.fap_preapp_request_date = updatedEntry.fap_preapp_request_date;
                    dbEntry.fap_preapp_submitted_date = updatedEntry.fap_preapp_submitted_date;
                    dbEntry.fap_preapp_receive_date = updatedEntry.fap_preapp_receive_date;
                    dbEntry.fap_app_sent_to_dr = updatedEntry.fap_app_sent_to_dr;
                    dbEntry.fap_Credentials_confirmed_date = updatedEntry.fap_Credentials_confirmed_date;
                    dbEntry.fap_credential_specialist = updatedEntry.fap_credential_specialist;
                    dbEntry.fap_app_specialist = updatedEntry.fap_app_specialist;
                    


                updatedPhyFacilities.Add(dbEntry);
                    /// TODO: uncomment once tested at dev
                    /*
                    // assuming override still valid
                    if (!dbEntry.fap_is_override)
                    {
                        dbEntry.fap_is_on_boarded = false;

                        // if is onboarded already, then set it
                        if (dbEntry.fap_onboarding_complete_provider_active_date.HasValue)
                            if (dbEntry.fap_onboarding_complete_provider_active_date.Value <= _createdDate)
                            {
                                dbEntry.fap_is_on_boarded = true;
                            }
                    }
                    */
                }

                /*
                var existingDBLicense = (from dbLic in _unitOfWork.FacilityPhysicianRepository.Query()
                                         join oldLic in existingEntries on dbLic.fap_user_key equals oldLic.fap_user_key
                                         select dbLic
                                        ).ToList();

                existingDBLicense.ForEach(x =>
                {
                    var updatedEntry = existingEntries.Where(y => y.fap_user_key == x.fap_user_key && y.fap_fac_key == x.fap_fac_key).First();

                    x.fap_start_date = updatedEntry.fap_start_date;
                    x.fap_end_date = updatedEntry.fap_end_date;
                });
                */
            }
            return updatedPhyFacilities;
        }

        private void CleanLists()
        {
            // clear temp storage
            _listOfPhysicians = new List<AspNetUser>();
            _listOfCredentials = new List<physician_license>();
            //_listOfFacilities = new List<facility>();
            _listOfAssociation = new List<facility_physician>();

            _listOfPhysicianDetails = new List<AspNetUser_Detail>();
        }

        public async Task GetAll(string requestId, string userId, string userFullName)
        {
            this._createdBy = userId;
            this._createdByName = userFullName;
            this._requestId = requestId;

            //await AddLogEntry("INFO", requestId, "Updating Facility Onboarding Override.", this._createdByName, null);

            // reset onboarding override if timer is ellapsed
            /// TODO: uncomment once tested at dev
            //_unitOfWork.ExecuteSqlCommand("UPDATE facility_physician SET fap_is_override = 0, fap_override_start = NULL, fap_override_hours = NULL WHERE  (DATEADD(HOUR, fap_override_hours, fap_override_start) <= '" + DateTime.Now.ToEST() + "') AND fap_override_start IS NOT NULL  ");

            // get all existing facilities
            var temp = _unitOfWork.FacilityRepository.Query().Select(x => new { x.fac_key, x.fac_name, x.fac_md_staff_source_name }).ToList();


            this._listOfDBFacilities = temp.Select(x => new facility { fac_key = x.fac_key, fac_name = x.fac_name, fac_md_staff_source_name = x.fac_md_staff_source_name }).ToList();

            // get IDs for all providers
            string allProvidersJson = await this.GetAllProviderIDs();

            if (string.IsNullOrEmpty(allProvidersJson))
            {
                //return "There is no data to process";
                await AddLogEntry("INFO", requestId, "This is no data to process", this._createdByName, null);
            }

            // convert to object
            dynamic providerList = JsonConvert.DeserializeObject(allProvidersJson);
            var inactiveProviders = this._unitOfWork.UserRepository.Query().Where(m => !m.IsActive).Select(m => m.Id).ToList();

            if (providerList != null)
            {

                await AddLogEntry("INFO", requestId, "Total Provider to Process = " + Convert.ToString(providerList.Count), this._createdByName, "");

                // process all providers
                foreach (var item in providerList)
                {
                    string providerID = Convert.ToString(item.ProviderID);
                    bool isInActive = inactiveProviders.Where(m => m == providerID).Any();
                    if (!isInActive)
                    {


                    tryAgain:

                        if (!string.IsNullOrEmpty(providerID))
                        {
                            try
                            {
                                await AddLogEntry("INFO", requestId, "Data import started for provider = " + providerID, this._createdByName, providerID);

                                var physician = await GetMDStaffData(providerID);
                                if (physician != null)
                                {
                                    await SaveMDStaffData(providerID);


                                    await AddLogEntry("INFO", requestId, "Data import completed for provider = " + this._currentPhysician, this._createdByName, providerID);
                                }
                                else
                                {
                                    await MarkUserInactive(providerID);
                                    await AddLogEntry("INFO", requestId, "Data import skipped for provider = " + providerID + " due to no ProviderTelespecialistEmail or ContractEnd Date", this._createdByName, providerID);
                                }
                            }
                            catch (WebException exWeb)
                            {
                                try { _unitOfWork.Rollback(); } catch { }

                                if (exWeb.Message == "The remote server returned an error: (401) Unauthorized.")
                                {
                                    ResetToken();

                                    await RefreshToken();

                                    goto tryAgain;
                                }
                            }
                            catch (Exception ex)
                            {
                                try { _unitOfWork.Rollback(); } catch { }

                                await AddLogEntry("ERROR", requestId, ex.ToString(), this._createdByName, providerID);
                            }
                            finally
                            {
                                CleanLists();
                            }
                        }
                    }
                    else
                    {
                        await AddLogEntry("INFO", requestId, "Privier = " + providerID + " is inactive", this._createdByName, "");
                    }
                } // end of foreach loop

                // update credential count in users
                _unitOfWork.ExecuteSqlCommand("UPDATE AspNetUsers SET CredentialCount = (SELECT COUNT(*) FROM facility_physician WHERE fap_user_key = AspNetUsers.Id and fap_is_on_boarded = 1 AND fap_is_active = 1 AND(GETDATE() BETWEEN  fap_start_date AND fap_end_date) )");
            }

            await this.DataSyncComplete(requestId);
        }

        private async Task DataSyncComplete(string requestId)
        {
            await AddLogEntry("COMPLETED", _requestId, "MD Staff data sync is complete.", _createdByName, "");
        }

        public List<Dictionary<string, object>> GetImportLogs(string requestId)
        {
            return _unitOfWork.SqlToList(string.Format(" exec data_import_log_get '{0}'", requestId)).ToList();
        }

        /// <summary>
        /// https://condadogroup.atlassian.net/browse/TCARE-153
        /// </summary>
        public bool IsActive
        {
            get
            {
                return _unitOfWork.AppSettingRepository.Query().FirstOrDefault().aps_is_md_staff_active;
            }
        }

        private async Task SaveMDStaffHospitals(string providerID)
        {
            await AddLogEntry("INPROGRESS", _requestId, "Getting MD Staff provider hospitals - " + _currentPhysician, this._createdByName, providerID);

            // get provider details
            string resultJson = await this.GetHospital(providerID);

            if (!string.IsNullOrEmpty(resultJson))
            {
                await AddLogEntry("INPROGRESS", _requestId, "Parsing MD Staff provider hospitals - " + _currentPhysician, this._createdByName, providerID);

                // convert to object
                JArray jsonObject = JArray.Parse(resultJson);

                // if converted to object
                if (jsonObject != null && jsonObject.Count() > 0)
                {
                    foreach (JObject item in jsonObject.Children<JObject>())
                    {
                        string query = "";
                        var referenceSource = ((JObject)item.Property("ReferenceSource").Value);
                        if (referenceSource != null)
                        {
                            try
                            {
                                string ID = Convert.ToString(referenceSource.Property("ID").Value);
                                string ReferenceSourceID = Convert.ToString(referenceSource.Property("ReferenceSourceID").Value);
                                string ReferenceType = Convert.ToString(referenceSource.Property("ReferenceType").Value);
                                string Code = Convert.ToString(referenceSource.Property("Code").Value);
                                string Name = Convert.ToString(referenceSource.Property("Name").Value);
                                string Address = Convert.ToString(referenceSource.Property("Address").Value);
                                string Address2 = Convert.ToString(referenceSource.Property("Address2").Value);
                                string City = Convert.ToString(referenceSource.Property("City").Value);
                                string State = Convert.ToString(referenceSource.Property("State").Value);
                                string Zip = Convert.ToString(referenceSource.Property("Zip").Value);
                                string Telephone = Convert.ToString(referenceSource.Property("Telephone").Value);
                                string Fax = Convert.ToString(referenceSource.Property("Fax").Value);
                                string Email = Convert.ToString(referenceSource.Property("Email").Value);
                                string URL = Convert.ToString(referenceSource.Property("URL").Value);
                                string LastUpdated = Convert.ToString(referenceSource.Property("LastUpdated").Value);
                                string UpdatedBy = Convert.ToString(referenceSource.Property("UpdatedBy").Value);
                                string Comments = Convert.ToString(referenceSource.Property("Comments").Value);
                                string Country = Convert.ToString(referenceSource.Property("Country").Value);
                                string County = Convert.ToString(referenceSource.Property("County").Value);
                                string TaxID = Convert.ToString(referenceSource.Property("TaxID").Value);
                                string Manager = Convert.ToString(referenceSource.Property("Manager").Value);
                                string Uid = Convert.ToString(referenceSource.Property("Uid").Value);
                                string Type = Convert.ToString(referenceSource.Property("Type").Value);
                                string CountryID = Convert.ToString(referenceSource.Property("CountryID").Value);
                                string CountyID = Convert.ToString(referenceSource.Property("CountyID").Value);
                                string TypesList = Convert.ToString(referenceSource.Property("TypesList").Value);
                                string AddressBlock = Convert.ToString(referenceSource.Property("AddressBlock").Value);

                                string IsArchived = Convert.ToString(referenceSource.Property("IsArchived").Value);
                                string IsNew = Convert.ToString(referenceSource.Property("IsNew").Value);
                                string IsLoaded = Convert.ToString(referenceSource.Property("IsLoaded").Value);

                                if (IsArchived.ToLower() == "true") IsArchived = "1"; else IsArchived = "0";
                                if (IsNew.ToLower() == "true") IsNew = "1"; else IsNew = "0";
                                if (IsLoaded.ToLower() == "true") IsLoaded = "1"; else IsLoaded = "0";

                                query = @"
                                IF NOT EXISTS (SELECT * FROM MDStaffFacility where ReferenceSourceID = '" + ReferenceSourceID + @"')
                                BEGIN

                                insert into MDStaffFacility ( 
                                            ID ,
                                            ReferenceSourceID ,
                                            ReferenceType ,
                                            Code ,
                                            Name ,
                                            Address ,
                                            Address2 ,
                                            City ,
                                            State ,
                                            Zip ,
                                            Telephone ,
                                            Fax ,
                                            Email ,
                                            URL ,
                                            LastUpdated ,
                                            UpdatedBy ,
                                            Comments ,
                                            Country ,
                                            County ,
                                            TaxID ,
                                            Manager ,
                                            Uid ,
                                            Type ,
                                            CountryID ,
                                            CountyID ,
                                            TypesList ,
                                            AddressBlock ,
                                            IsArchived ,
                                            IsNew ,
                                            IsLoaded ,
                                            ImportDateUTC 
                                )

                                Values ( " + Environment.NewLine;
                                query += $"'{ID}'," + Environment.NewLine;
                                query += $"'{ReferenceSourceID}'," + Environment.NewLine;
                                query += $"'{ReferenceType}'," + Environment.NewLine;
                                query += $"'{Code.Replace("'", "''")}'," + Environment.NewLine;
                                query += $"'{Name.Replace("'", "''")}'," + Environment.NewLine;
                                query += $"'{Address.Replace("'", "''")}'," + Environment.NewLine;
                                query += $"'{Address2.Replace("'", "''")}'," + Environment.NewLine;
                                query += $"'{City}' ," + Environment.NewLine;
                                query += $"'{State}' ," + Environment.NewLine;
                                query += $"'{Zip}' ," + Environment.NewLine;
                                query += $"'{Telephone}' ," + Environment.NewLine;
                                query += $"'{Fax}' ," + Environment.NewLine;
                                query += $"'{Email}'," + Environment.NewLine;
                                query += $"'{URL}' ," + Environment.NewLine;
                                query += $"'{LastUpdated}' ," + Environment.NewLine;
                                query += $"'{UpdatedBy}' ," + Environment.NewLine;
                                query += $"'{Comments.Replace("'", "''")}' ," + Environment.NewLine;
                                query += $"'{Country}' ," + Environment.NewLine;
                                query += $"'{County}' ," + Environment.NewLine;
                                query += $"'{TaxID}' ," + Environment.NewLine;
                                query += $"'{Manager}' ," + Environment.NewLine;
                                query += $"'{Uid}' ," + Environment.NewLine;
                                query += $"'{Type}' ," + Environment.NewLine;
                                query += $"'{CountryID}' ," + Environment.NewLine;
                                query += $"'{CountyID}' ," + Environment.NewLine;
                                query += $"'{TypesList}' ," + Environment.NewLine;
                                query += $"'{AddressBlock.Replace("'", "''")}' ," + Environment.NewLine;
                                query += $"{IsArchived} ," + Environment.NewLine;
                                query += $"{IsNew} ," + Environment.NewLine;
                                query += $"{IsLoaded} ," + Environment.NewLine;
                                query += "GETUTCDATE()" + Environment.NewLine;
                                query += ")" + Environment.NewLine;
                                query += "END";


                                this._unitOfWork.ExecuteSqlCommand(query);
                            }
                            catch (Exception ex)
                            {
                                
                                await AddLogEntry("ERROR", _requestId, "Getting MD Staff provider hospitals - " + _currentPhysician+" - "+ex.ToString(), this._createdByName, providerID);
                            }
                        }
                    }
                }
            }
        }

        public async Task GetAllHospitials(string requestId, string userId, string userFullName)
        {
            this._createdBy = userId;
            this._createdByName = userFullName;
            this._requestId = requestId;

            // get IDs for all providers
            string allProvidersJson = await this.GetAllProviderIDs();

            if (string.IsNullOrEmpty(allProvidersJson))
            {
                //return "There is no data to process";
                await AddLogEntry("INFO", requestId, "This is no data to process", this._createdByName, null);
            }

            // convert to object
            dynamic providerList = JsonConvert.DeserializeObject(allProvidersJson);

            if (providerList != null)
            {
                await AddLogEntry("INFO", requestId, "Total Provider to Process = " + Convert.ToString(providerList.Count), this._createdByName, "");

                // process all providers
                foreach (var item in providerList)
                {
                    string providerID = Convert.ToString(item.ProviderID);

                tryAgain:

                    if (!string.IsNullOrEmpty(providerID))
                    {
                        try
                        {
                            await AddLogEntry("INFO", requestId, "Facility import started for provider = " + providerID, this._createdByName, providerID);

                            await SaveMDStaffHospitals(providerID);

                            await AddLogEntry("INFO", requestId, "Facility import completed for provider = " + this._currentPhysician, this._createdByName, providerID);
                        }
                        catch (WebException exWeb)
                        {
                            try { _unitOfWork.Rollback(); } catch { }

                            if (exWeb.Message == "The remote server returned an error: (401) Unauthorized.")
                            {
                                ResetToken();

                                await RefreshToken();

                                goto tryAgain;
                            }
                        }
                        catch (Exception ex)
                        {
                            try { _unitOfWork.Rollback(); } catch { }

                            await AddLogEntry("ERROR", requestId, ex.ToString(), this._createdByName, providerID);
                        }
                        finally
                        {
                            CleanLists();
                        }
                    }
                }
            }

            await this.DataSyncComplete(requestId);
        }

        #region ----- IDispose -----

        private bool disposed = false; // to detect redundant calls
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // dispose-only, i.e. non-finalizable logic
                    _listOfPhysicians = null;
                    _listOfCredentials = null;
                    _listOfAssociation = null;
                    _listOfDBFacilities = null;
                }
                disposed = true;
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
