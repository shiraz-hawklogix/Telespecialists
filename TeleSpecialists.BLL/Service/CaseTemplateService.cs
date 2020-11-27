using System.Collections.Generic;
using System.Linq;
using System.Text;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class CaseTemplateService : BaseService
    {
        private string htmlDocument = "";
        private PhysicianService _physicianService = new PhysicianService();
        private UCLService _uclService = new UCLService();
        private NIHStrokeScaleService _nihsStrokeScaleService = new NIHStrokeScaleService();
        bool disposed = false;

        public string GetIllnessHistory(@case model, EntityTypes entityType)
        {
            StringBuilder strIllnessHistory = new StringBuilder();
            bool isNonTpaTemplate = (entityType == EntityTypes.StrokeAlertTemplateNoTpa || entityType == EntityTypes.StrokeAlertTemplateNoTpaTeleStroke) ? true : false;

            if (model.cas_patient_type > 0)
            {
                strIllnessHistory.Append("<br/>");
                var workFlowType = (PatientType)model.cas_patient_type;
                if (workFlowType == PatientType.EMS)
                {
                    strIllnessHistory.Append("Patient was brought by EMS");
                    if (!string.IsNullOrEmpty(model.cas_metric_symptoms))
                    {
                        strIllnessHistory.AppendItem($" for symptoms of { (!string.IsNullOrEmpty(model.cas_metric_symptoms) ? model.cas_metric_symptoms.Replace("<br/>", "") : "")} <br/>", model.cas_metric_symptoms);
                    }
                    else
                    {
                        strIllnessHistory.Append(". <br/>");
                    }
                }
                else if (workFlowType == PatientType.Triage)
                {
                    strIllnessHistory.Append("Patient was brought by private transportation");
                    if (!string.IsNullOrEmpty(model.cas_metric_symptoms))
                    {
                        strIllnessHistory.AppendItem($" with symptoms of { (!string.IsNullOrEmpty(model.cas_metric_symptoms) ? model.cas_metric_symptoms.Replace("<br/>", "") : "")} <br/>", model.cas_metric_symptoms);
                    }
                    else
                    {
                        strIllnessHistory.Append(". <br/>");
                    }
                }
                else if (workFlowType == PatientType.Inpatient)
                {
                    strIllnessHistory.Append("Inpatient stroke alert was called");
                    if (!string.IsNullOrEmpty(model.cas_metric_symptoms))
                    {
                        strIllnessHistory.AppendItem($" for symptoms of { (!string.IsNullOrEmpty(model.cas_metric_symptoms) ? model.cas_metric_symptoms.Replace("<br/>", "") : "")} <br/>", model.cas_metric_symptoms);
                    }
                    else
                    {
                        strIllnessHistory.Append(". <br/>");
                    }
                }
            }
            if (!string.IsNullOrEmpty(model.cas_metric_hpi))
            {
                strIllnessHistory.Append("<br/>");
                strIllnessHistory.Append(model.cas_metric_hpi).Append("<br/>");
            }
            //TCARE-559 removing CT Head from here.
            //List<string> listCTHead = new List<string>();
            //if (model.cas_metric_ct_head_has_no_acture_hemorrhage)
            //{
            //    listCTHead.Add("CT head showed no acute hemorrhage or acute core infarct.");
            //}
            //if (model.cas_metric_ct_head_is_reviewed)
            //{
            //    listCTHead.Add("CT head was reviewed.");
            //}
            //if (model.cas_metric_ct_head_is_not_reviewed)
            //{
            //    listCTHead.Add("CT head was not reviewed.");
            //}

            //if (listCTHead.Count() > 0)
            //    strIllnessHistory.Append($"<br/>{ string.Join("<br/>", listCTHead)}<br/>");

            strIllnessHistory.Append("<br/>");

            //if (isNonTpaTemplate)
            //{
            //    if (model.cas_metric_last_seen_normal == LB2S2CriteriaOptions.Yes.ToInt()) strIllnessHistory.Append("Last Seen Normal was within 4.5 hours.<br/>");
            //    else if (model.cas_metric_last_seen_normal == LB2S2CriteriaOptions.No.ToInt()) strIllnessHistory.Append("Last Seen Normal was beyond 4.5 hours of presentation.<br/>");
            //}
            //else
            //{
            //    if (model.cas_metric_last_seen_normal == LB2S2CriteriaOptions.Yes.ToInt()) strIllnessHistory.Append("Last Seen Normal was outside of 4.5 hours.<br/>");
            //    else if (model.cas_metric_last_seen_normal == LB2S2CriteriaOptions.No.ToInt()) strIllnessHistory.Append("Last Seen Normal was within 4.5 hours of presentation.<br/>");
            //}
            if(model.cas_ctp_key != 10)
            {
                if (model.cas_metric_last_seen_normal == LB2S2CriteriaOptions.Yes.ToInt()) strIllnessHistory.Append("Last seen normal was beyond 4.5 hours of presentation.<br/>");
                else if (model.cas_metric_last_seen_normal == LB2S2CriteriaOptions.No.ToInt()) strIllnessHistory.Append("Last seen normal was within 4.5 hours.<br/>");

                if (model.cas_metric_has_hemorrhgic_history == LB2S2CriteriaOptions.Yes.ToInt()) strIllnessHistory.Append("There is history of hemorrhagic complications or intracranial hemorrhage.<br/>");
                else if (model.cas_metric_has_hemorrhgic_history == LB2S2CriteriaOptions.No.ToInt()) strIllnessHistory.Append("There is no history of hemorrhagic complications or intracranial hemorrhage.<br/>");

                if (model.cas_metric_has_recent_anticoagulants == LB2S2CriteriaOptions.Yes.ToInt()) strIllnessHistory.Append("There is history of Recent Anticoagulants.<br/>");
                else if (model.cas_metric_has_recent_anticoagulants == LB2S2CriteriaOptions.No.ToInt()) strIllnessHistory.Append("There is no history of Recent Anticoagulants.<br/>");

                if (model.cas_metric_has_major_surgery_history == LB2S2CriteriaOptions.Yes.ToInt()) strIllnessHistory.Append("There is no history of recent major surgery.<br/>");
                else if (model.cas_metric_has_major_surgery_history == LB2S2CriteriaOptions.No.ToInt()) strIllnessHistory.Append("There is no history of recent major surgery.<br/>");

                if (model.cas_metric_has_stroke_history == LB2S2CriteriaOptions.Yes.ToInt()) strIllnessHistory.Append("There is history of recent stroke.<br/>");
                else if (model.cas_metric_has_stroke_history == LB2S2CriteriaOptions.No.ToInt()) strIllnessHistory.Append("There is no history of recent stroke.<br/>");

            }

            return strIllnessHistory.ToString();
        }
        public string GetPhysicianMetrics(@case model)
        {
            var loginDelayReason = "";
            var tpaDelayReason = "";

            if (model.cas_billing_lod_key.HasValue)
                loginDelayReason = _uclService.GetDetails(model.cas_billing_lod_key.Value).ucd_description.Replace("<br/>", "");
            if (model.cas_metric_tpaDelay_key.HasValue)
                tpaDelayReason = _uclService.GetDetails(model.cas_metric_tpaDelay_key.Value).ucd_description.Replace("<br/>", "");

            PatientType patientType = PatientType.EMS;
            if (model.cas_patient_type > 0)
                patientType = ((PatientType)model.cas_patient_type);
            StringBuilder strIllnessHistory = new StringBuilder();
            string lastKnownWell = model.cas_metric_is_lastwell_unknown ? "Unknown" : model.cas_metric_lastwell_date_est?.FormatDateTime();
            strIllnessHistory.AppendItem($"<div class='datetime' >Last Known Well: {lastKnownWell}</div>", lastKnownWell);
            strIllnessHistory.AppendItem($"<div class='datetime' >TeleSpecialists Notification Time: {model.cas_response_ts_notification?.FormatDateTime()}</div>", model.cas_response_ts_notification?.FormatDateTime());
            if (patientType != PatientType.Inpatient)
            {
                var title = "Arrival Time:";

                strIllnessHistory.AppendItem($"<div class='datetime' >{title} {model.cas_metric_door_time_est?.FormatDateTime()}</div>", model.cas_metric_door_time_est?.FormatDateTime());
            }

            if (model.cas_metric_symptom_onset_during_ed_stay_time_est.HasValue)
            {
                var title = "Symptom Onset During ED Stay:";
                strIllnessHistory.AppendItem($"<div class='datetime' >{title} {model.cas_metric_symptom_onset_during_ed_stay_time_est?.FormatDateTime()}</div>", model.cas_metric_symptom_onset_during_ed_stay_time_est?.FormatDateTime());
            }

            strIllnessHistory.AppendItem($"<div class='datetime' >Stamp Time: {model.cas_metric_stamp_time_est?.FormatDateTime()}</div>", model.cas_metric_stamp_time_est?.FormatDateTime());
            if (model.cas_callback_response_time_est.HasValue)
                strIllnessHistory.AppendItem($"<div class='datetime' >Callback Response Time: {model.cas_callback_response_time_est?.FormatDateTime()}</div>", model.cas_callback_response_time_est?.FormatDateTime());

            if (model.cas_phy_has_technical_issue && model.cas_phy_technical_issue_date_est.HasValue)
            {
                strIllnessHistory.AppendItem($"<div class='datetime' >Telephone Response Time: {model.cas_phy_technical_issue_date_est?.FormatDateTime()}</div>", model.cas_phy_technical_issue_date_est?.FormatDateTime());
            }

            strIllnessHistory.AppendItem($"<div class='datetime' >Time First Login Attempt: {model.cas_response_first_atempt?.FormatDateTime()}</div>", model.cas_response_first_atempt?.FormatDateTime());
            if (model.cas_ctp_key == 10)
            {
                strIllnessHistory.AppendItem($"<div class='datetime' >Video Start Time: {model.cas_metric_video_start_time_est?.FormatDateTime()}</div>", model.cas_metric_video_start_time_est?.FormatDateTime());
            }
            else
            {
                strIllnessHistory.AppendItem($"<div class='datetime' >Video Start Time: {model.cas_metric_video_start_time_est?.FormatDateTime()}</div><br/>", model.cas_metric_video_start_time_est?.FormatDateTime());
            }

            if (model.cas_ctp_key != 10)
            {
                strIllnessHistory.AppendItem($"Symptoms: { (!string.IsNullOrEmpty(model.cas_metric_symptoms) ? model.cas_metric_symptoms.Replace("<br/>", "") : "")} <br/>", model.cas_metric_symptoms);
                strIllnessHistory.AppendItem($"<div class='datetime' >NIHSS Start Assessment Time: {model.cas_metric_assesment_time_est?.FormatDateTime()}</div>", model.cas_metric_assesment_time_est?.FormatDateTime());
                // Ignoring patient history
                strIllnessHistory.AppendItem($"<div class='datetime' >Alteplase Early Mix Decision Time: {model.cas_metric_tpa_verbal_order_time_est?.FormatDateTime()}</div>", model.cas_metric_tpa_verbal_order_time_est?.FormatDateTime());

                if (model.cas_metric_tpa_consult.ToBool())
                    strIllnessHistory.Append($"Patient is a candidate for Alteplase/Activase. <br/>");
                else
                    strIllnessHistory.Append($"Patient is not a candidate for Alteplase/Activase. <br/>");
            }
            if (model.cas_ctp_key != 10) {
                if (model.cas_metric_non_tpa_reason_key != null && model.cas_metric_non_tpa_reason_key > 0)
                {
                    var tpaReason = _uclService.GetDetails(model.cas_metric_non_tpa_reason_key.ToInt());
                    if (tpaReason.ucd_title.ToLower().Equals("other") && !string.IsNullOrEmpty(model.cas_metric_non_tpa_reason_text))
                        strIllnessHistory.Append($"Patient was not deemed candidate for Alteplase/Activase thrombolytics because of {model.cas_metric_non_tpa_reason_text}. <br/>");
                    else
                        strIllnessHistory.Append($"Patient was not deemed candidate for Alteplase/Activase thrombolytics because of {tpaReason.ucd_title}. <br/>");
                }
            }
            if (model.cas_ctp_key != 10)
            {
                strIllnessHistory.AppendItem($"<div class='datetime' >Alteplase/Activase CPOE Order Time: {model.cas_metric_pa_ordertime_est?.FormatDateTime()}</div>", model.cas_metric_pa_ordertime_est?.FormatDateTime());
                strIllnessHistory.AppendItem($"<div class='datetime' >Needle Time: {model.cas_metric_needle_time_est?.FormatDateTime()}</div>", model.cas_metric_needle_time_est?.FormatDateTime());
                strIllnessHistory.AppendItem($"Weight Noted by Staff: {(model.cas_metric_weight.HasValue ? model.cas_metric_weight.ToString() + " " + model.cas_metric_weight_unit : "")} <br/>", model.cas_metric_weight?.ToString());
            }
            strIllnessHistory.AppendItem($"<div class='datetime' >Video End Time: {model.cas_metric_video_end_time_est?.FormatDateTime()}</div>", model.cas_metric_video_end_time_est?.FormatDateTime());

            if (model.cas_metric_tpa_consult)
            {
                if (model.cas_metric_door_time_est.HasValue && model.cas_metric_needle_time_est.HasValue)
                {
                    if (System.Math.Abs((model.cas_metric_door_time_est.Value.Subtract(model.cas_metric_needle_time_est.Value)).TotalMinutes) >= 45)
                    {
                        if (model.cas_metric_tpaDelay_key.HasValue)
                       {
                             var tpaDelayReasons = _uclService.GetDetails(model.cas_metric_tpaDelay_key.Value);
                           var reason = tpaDelayReasons.ucd_description;
                             strIllnessHistory.AppendItem($"<div>Reason for Alteplase/Activase Delay: {reason}</div>", reason);
                         }
                    }
                }
            }



            List<string> listCTHead = new List<string>();
            List<string> listAdvanceImaging = new List<string>();
            List<string> ThrombectomyList = new List<string>();

            if (model.cas_metric_ct_head_has_no_acture_hemorrhage)
                listCTHead.Add("CT head showed no acute hemorrhage or acute core infarct.");
            if (model.cas_metric_ct_head_is_reviewed)
            {
                if (string.IsNullOrEmpty(model.cas_metric_ct_head_reviewed_text))
                    listCTHead.Add("CT head was reviewed." + model.cas_metric_ct_head_reviewed_text);
                else
                    listCTHead.Add("CT head was reviewed and results were: " + model.cas_metric_ct_head_reviewed_text);
            }
            if (model.cas_metric_ct_head_is_not_reviewed)

                listCTHead.Add("CT head was not reviewed.");
            if (listCTHead.Count() > 0)
                strIllnessHistory.Append($"<br/>{string.Join("<br/> \t", listCTHead)} <br/>");

            if (model.cas_metric_thrombectomy_medical_decision_making == ThrombectomyMedicalDecisionMaking.ClinicalPresentationIsNotSuggestiveOfLargeVesselOcclusiveDisease_PatientIsNotACandidateForThrombectomy.ToInt())
            {
                var text = ThrombectomyMedicalDecisionMaking.ClinicalPresentationIsNotSuggestiveOfLargeVesselOcclusiveDisease_PatientIsNotACandidateForThrombectomy.ToDescription();
                ThrombectomyList.Add(text);
            }

            if (model.cas_metric_thrombectomy_medical_decision_making == ThrombectomyMedicalDecisionMaking.LowerLikelihoodOfLargeVesselOcclusiveButFollowingStatStudiesAreRecommended.ToInt())
            {
                var text = ThrombectomyMedicalDecisionMaking.LowerLikelihoodOfLargeVesselOcclusiveButFollowingStatStudiesAreRecommended.ToDescription();
                ThrombectomyList.Add(text);
            }

            if (model.cas_metric_thrombectomy_medical_decision_making == ThrombectomyMedicalDecisionMaking.ClinicalPresentationIsSuggestiveOfLargeVesselOcclusiveDisease_RecommendationsAreAsFollows.ToInt())
            {
                var text = ThrombectomyMedicalDecisionMaking.ClinicalPresentationIsSuggestiveOfLargeVesselOcclusiveDisease_RecommendationsAreAsFollows.ToDescription();
                ThrombectomyList.Add(text);
            }

            if (ThrombectomyList.Count() > 0)
                strIllnessHistory.Append($"<br/>{string.Join("<br/> \t", ThrombectomyList)} <br/>");

            if (model.cas_metric_advance_imaging_cta_head_and_neck)
            {
                listAdvanceImaging.Add("CTA Head and Neck.");
            }

            if (model.cas_metric_advance_imaging_ct_perfusion)
            {
                listAdvanceImaging.Add("CT Perfusion.");
            }

            if (model.cas_metric_advance_imaging_to_be_reviewed_by_ed_provider_and_nir)
            {
                listAdvanceImaging.Add("Advanced Imaging to be Reviewed by ED Provider and NIR.");
            }

            if (model.cas_metric_advance_imaging_is_suggestive_of_large_vessel_occlusion)
            {
                listAdvanceImaging.Add("Advanced Imaging is Suggestive of Large Vessel Occlusion, Neurointerventional Specialist to be Consulted.");
            }

            if (model.cas_metric_advance_imaging_reviewed_no_indication_of_large_vessel_occlusive_thrombus)
            {
                listAdvanceImaging.Add("Reviewed, No Indication of Large Vessel Occlusive Thrombus, Patient is not an NIR Candidate.");
            }

            if (listAdvanceImaging.Count() > 0)
                strIllnessHistory.Append($"<br/>{string.Join("<br/> \t", listAdvanceImaging)} <br/><br/>");

            //Added for new fields - ticket - 364 : Start

            if (model.cas_metric_radiologist_callback_for_review_of_advance_imaging == true && model.cas_metric_radiologist_callback_for_review_of_advance_imaging_date != null)
            {
                strIllnessHistory.Append("<br />Radiologist was called back for review of advanced imaging on " + model.cas_metric_radiologist_callback_for_review_of_advance_imaging_date?.FormatDateTime());
            }
            else if (model.cas_metric_radiologist_callback_for_review_of_advance_imaging == false && model.cas_metric_radiologist_callback_for_review_of_advance_imaging_notes != null)
            {
                strIllnessHistory.Append("<br />Radiologist was not called back for review of advanced imaging because " + model.cas_metric_radiologist_callback_for_review_of_advance_imaging_notes);

            }
            //if (model.cas_metric_is_neuro_interventional.ToBool())
            //{
            //    strIllnessHistory.Append("This is deemed to be a candidate for thrombectomy.<br/>");
            //    if (model.cas_metric_discussed_with_neurointerventionalist.ToBool())
            //        strIllnessHistory.Append("Case was discussed with Neurointerventionalist.<br/>");
            //    else
            //        strIllnessHistory.Append("Case not discussed with Neurointerventionalist.<br/>");
            //}
            if (model.cas_metric_is_neuro_interventional != null && model.cas_metric_is_neuro_interventional == true)
            {
                if (model.cas_metric_discussed_with_neurointerventionalist == true && model.cas_metric_discussed_with_neurointerventionalist_date != null)
                {
                    strIllnessHistory.Append("<br />Discussed with Neurointerventionalist on " + model.cas_metric_discussed_with_neurointerventionalist_date?.FormatDateTime());
                }
                else if (model.cas_metric_discussed_with_neurointerventionalist == false && model.cas_metric_discussed_with_neurointerventionalist_notes != null)
                {
                    strIllnessHistory.Append("<br />Not discussed with neurointerventionalist because " + model.cas_metric_discussed_with_neurointerventionalist_notes);
                }
            }

            if (patientType != PatientType.Inpatient)
            {
                bool __cas_metric_physician_notified_of_thrombolytics = false;
                if (model.cas_metric_physician_notified_of_thrombolytics.HasValue && model.cas_metric_physician_notified_of_thrombolytics.Value)
                {
                    __cas_metric_physician_notified_of_thrombolytics = true;
                }

                if (__cas_metric_physician_notified_of_thrombolytics == true && model.cas_metric_physician_notified_of_thrombolytics_date != null)
                {
                    strIllnessHistory.Append("<br /> ED Physician notified of diagnostic impression and management plan on " + model.cas_metric_physician_notified_of_thrombolytics_date?.FormatDateTime());
                }
                else if (__cas_metric_physician_notified_of_thrombolytics == false && model.cas_metric_physician_notified_of_thrombolytics_notes != null)
                {
                    strIllnessHistory.Append("<br /> ED Physician not notified of diagnostic impression and management plan because " + model.cas_metric_physician_notified_of_thrombolytics_notes);
                }
            }

            //Added for new fields - ticket - 364 : End

            return strIllnessHistory.ToString();
        }
        public string GenerateNIHSSReport(@case model)
        {
            //string nihssMessage = "NIHSS cannot be completed due to patient status.";
            //if (model.case_template_stroke_neuro_tpa != null && model.case_template_stroke_neuro_tpa.csn_ignore_nihss)
            ////if (model.case_template_stroke_neuro_tpa != null && model.cas_nihss_cannot_completed)
              ////  return nihssMessage;
            //else if (model.case_template_stroke_notpa != null && model.case_template_stroke_notpa.ctn_ignore_nihss)
            ////else if (model.case_template_stroke_notpa != null && model.cas_nihss_cannot_completed)
                ////return nihssMessage;
            //else if (model.case_template_stroke_tpa != null && model.case_template_stroke_tpa.cts_ignore_nihss)
            ////else if (model.case_template_stroke_tpa != null && model.cas_nihss_cannot_completed)
                ////return nihssMessage;
            //else if (model.case_template_telestroke_notpa != null && model.case_template_telestroke_notpa.ctt_ignore_nihss)
            ////else if (model.case_template_telestroke_notpa != null && model.cas_nihss_cannot_completed)
                ////return nihssMessage;
            ////else
            ////{
                var paragraph = new StringBuilder();
                if (!string.IsNullOrEmpty(model.SelectedNIHSQuestionResponse))
                {
                    var selectedAnswers = model.SelectedNIHSQuestionResponse.Split(',').Select(m => m.ToInt()).ToList();
                    var responseList = _nihsStrokeScaleService.GetSelectedOptions(selectedAnswers).OrderBy(m => m.nss_nsq_key).ToList();
                    foreach (var item in responseList)
                    {
                        paragraph.Append(item.nih_stroke_scale_question.nsq_title + " - " + item.nss_title);
                        paragraph.Append("<b> + " + item.nss_score.ToString() + "</b><br/>");
                    }
                ////}
                ////return paragraph.ToString();
            }
            return paragraph.ToString();
        }

        private void ReplaceText(string inputText, string textToReplace)
        {
            htmlDocument = htmlDocument.Replace(inputText, textToReplace);
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // dispose-only, i.e. non-finalizable logic
                    _physicianService.Dispose();
                    _uclService.Dispose();
                    _nihsStrokeScaleService.Dispose();
                }
                disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}
