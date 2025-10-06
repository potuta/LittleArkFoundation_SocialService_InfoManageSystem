using HtmlAgilityPack;
using LittleArkFoundation.Areas.Admin.Models.Housing;
using LittleArkFoundation.Areas.Admin.Services.Html;
using Microsoft.AspNetCore.Html;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Net.Http;
using System.Text;

namespace LittleArkFoundation.Data
{
    public class HtmlTemplateService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ConnectionService _connectionService;

        public HtmlTemplateService(IWebHostEnvironment environment, ConnectionService connectionService)
        {
            _environment = environment;
            _connectionService = connectionService;
        }

        public async Task<string> ModifyHtmlTemplateAsync_Page1(string htmlContent, int id, int assessmentID)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using var context = new ApplicationDbContext(connectionString);

            var patient = await context.Patients.FindAsync(id);
            var assessment = await context.Assessments.FirstOrDefaultAsync(a => a.AssessmentID == assessmentID && a.PatientID == id);
            var referral = await context.Referrals.FirstOrDefaultAsync(r => r.AssessmentID == assessmentID && r.PatientID == id);
            var informant = await context.Informants.FirstOrDefaultAsync(i => i.AssessmentID == assessmentID && i.PatientID == id);
            var familymembers = await context.FamilyComposition
                                .Where(f => f.AssessmentID == assessmentID && f.PatientID == id)
                                .ToListAsync();
            var household = await context.Households.FirstOrDefaultAsync(h => h.AssessmentID == assessmentID && h.PatientID == id);
            var mswdclassification = await context.MSWDClassification.FirstOrDefaultAsync(m => m.AssessmentID == assessmentID && m.PatientID == id);
            
            if (patient == null)
            {
                return string.Empty;
            }

            // UPDATE LOGO IMAGE
            string imagePath = Path.Combine(_environment.WebRootPath, "resources", "NCH-Logo.png");
            byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(imagePath);
            string base64String = Convert.ToBase64String(imageBytes);
            htmlContent = htmlContent.Replace("/resources/NCH-Logo.png", $"data:image/png;base64,{base64String}");

            // USING HTMLAGILITYPACK
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // ASSESSMENTS
            htmlDoc.SetInnerHtml("//div[@class='Dateofinterview']", assessment.DateOfInterview.Safe());
            htmlDoc.SetInnerHtml("//div[@class='Timeofinterview']", assessment.TimeOfInterview.Safe());
            htmlDoc.SetInnerHtml("//div[@class='Basicward']", assessment.BasicWard);
            htmlDoc.SetInnerHtml("//div[@class='Nonbasicward']", assessment.NonBasicWard);
            htmlDoc.SetInnerHtml("//div[@class='Healthrecord']", assessment.HealthRecordNo);
            htmlDoc.SetInnerHtml("//div[@class='Mswdno']", assessment.MSWDNo);

            // REFERRALS
            htmlDoc.SetInnerHtml("//div[@class='Namereferral']", referral.Name);
            htmlDoc.SetInnerHtml("//div[@class='Addressreferral']", referral.Address);
            htmlDoc.SetInnerHtml("//div[@class='Contactnoreferral']", referral.ContactNo.Safe() == "0" ? "" : referral.ContactNo.Safe());

            // INFORMANTS
            htmlDoc.SetInnerHtml("//div[@class='Informantname']", informant.Name);
            htmlDoc.SetInnerHtml("//div[@class='Relationtopatient']", informant.RelationToPatient);
            htmlDoc.SetInnerHtml("//div[@class='Contactnoinformant']", informant.ContactNo.Safe() == "0" ? "" : informant.ContactNo.Safe());
            htmlDoc.SetInnerHtml("//div[@class='Addressinformant']", informant.Address);

            // PATIENTS
            htmlDoc.SetInnerHtml("//div[@class='Patientsurname']", patient.LastName);
            htmlDoc.SetInnerHtml("//div[@class='Patientfirstname']", patient.FirstName);
            htmlDoc.SetInnerHtml("//div[@class='Patientmiddlename']", patient.MiddleName);
            htmlDoc.SetInnerHtml("//div[@class='Dateofbirth']", patient.DateOfBirth.Safe());
            htmlDoc.SetInnerHtml("//div[@class='Age']", assessment.Age);

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Sexmalecheckbox", patient.Sex.Safe().ToUpper() == "MALE");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Sexfemalecheckbox", patient.Sex.Safe().ToUpper() == "FEMALE");

            htmlDoc.SetInnerHtml("//div[@class='Contactnopatient']", assessment.ContactNo);
            htmlDoc.SetInnerHtml("//div[@class='Placeofbirth']", patient.DateOfBirth.Safe());

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Gendermalecheckbox", assessment.Gender.Safe().ToUpper() == "MALE");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Genderfemalecheckbox", assessment.Gender.Safe().ToUpper() == "FEMALE");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Genderlgbtqcheckbox", assessment.Gender.Safe().ToUpper() == "LGBTQIA+");

            htmlDoc.SetInnerHtml("//div[@class='Religion']", assessment.Religion);
            htmlDoc.SetInnerHtml("//div[@class='Nationality']", patient.Nationality);
            htmlDoc.SetInnerHtml("//div[@class='Permanentaddress']", assessment.PermanentAddress);
            htmlDoc.SetInnerHtml("//div[@class='Temporaryaddress']", assessment.TemporaryAddress);

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Legitimatecheckbox", assessment.CivilStatus.Safe().ToUpper() == "LEGITIMATE");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Illegitimatecheckbox", assessment.CivilStatus.Safe().ToUpper() == "ILLEGITIMATE");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Marriedcheckbox", assessment.CivilStatus.Safe().ToUpper() == "MARRIED");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Commonlawcheckbox", assessment.CivilStatus.Safe().ToUpper() == "COMMON LAW");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Widowedcheckbox", assessment.CivilStatus.Safe().ToUpper() == "WIDOWED");

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Primarycheckbox", assessment.EducationLevel.Safe().ToUpper() == "PRIMARY");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Secondarycheckbox", assessment.EducationLevel.Safe().ToUpper() == "SECONDARY");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Vocationalcheckbox", assessment.EducationLevel.Safe().ToUpper() == "VOCATIONAL");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Tertiarycheckbox", assessment.EducationLevel.Safe().ToUpper() == "TERTIARY");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Noeducationcheckbox", assessment.EducationLevel.Safe().ToUpper() == "NO EDUCATIONAL ATTAINMENT");

            htmlDoc.SetInnerHtml("//div[@class='Occupation']", assessment.Occupation);
            htmlDoc.SetInnerHtml("//div[@class='Patientsmonthlyincome']", assessment.MonthlyIncome.Safe());
            htmlDoc.SetInnerHtml("//div[@class='Pin']", assessment.PhilhealthPIN);

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Directcontributorcheckbox", assessment.PhilhealthMembership.Safe().ToUpper() == "DIRECT CONTRIBUTOR");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Indirectcontributorcheckbox", assessment.PhilhealthMembership.Safe().ToUpper() == "INDIRECT CONTRIBUTOR");
            
            // FAMILY COMPOSITION
            int i = 1;
            foreach (var familyMember in familymembers)
            {
                htmlDoc.SetInnerHtml($"//div[@class='Familyname{i}']", familyMember.Name);
                htmlDoc.SetInnerHtml($"//div[@class='Familyage{i}']", familyMember.Age);
                htmlDoc.SetInnerHtml($"//div[@class='Familydateofbirth{i}']", familyMember.DateOfBirth.Safe());
                htmlDoc.SetInnerHtml($"//div[@class='Familycivilstatus{i}']", familyMember.CivilStatus);
                htmlDoc.SetInnerHtml($"//div[@class='Familyrelationshiptopatient{i}']", familyMember.RelationshipToPatient);

                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, $"Yesfamilylivingwithchild{i}", familyMember.LivingWithChild == true);
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, $"Nofamilylivingwithchild{i}", familyMember.LivingWithChild == false);

                htmlDoc.SetInnerHtml($"//div[@class='Familyeducationalattainment{i}']", familyMember.EducationalAttainment);
                htmlDoc.SetInnerHtml($"//div[@class='Familyoccupation{i}']", familyMember.Occupation);
                htmlDoc.SetInnerHtml($"//div[@class='Familymonthlyincome{i}']", familyMember.MonthlyIncome.Safe());

                i++;
            }

            htmlDoc.SetInnerHtml("//div[@class='Othersourcesofincome']", household.OtherSourcesOfIncome);
            htmlDoc.SetInnerHtml("//div[@class='Householdsize']", household.HouseholdSize.Safe());
            htmlDoc.SetInnerHtml("//div[@class='Totalhouseholdincome']", household.TotalHouseholdIncome.Safe());
            htmlDoc.SetInnerHtml("//div[@class='Percapitaincome']", household.PerCapitaIncome.Safe());

            // MSWD CLASSIFICATION
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Financiallycapablecheckbox", mswdclassification.MainClassification.Safe().ToUpper() == "FINANCIALLY CAPABLE/CAPACITATED");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Financiallyincapablecheckbox", mswdclassification.MainClassification.Safe().ToUpper() == "FINANCIALLY INCAPABLE/INCAPACITATED");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Indigentcheckbox", mswdclassification.MainClassification.Safe().ToUpper() == "INDIGENT");
            
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "C1checkbox", mswdclassification.SubClassification.Safe().ToUpper() == "C1");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "C2checkbox", mswdclassification.SubClassification.Safe().ToUpper() == "C2");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "C3checkbox", mswdclassification.SubClassification.Safe().ToUpper() == "C3");

            switch (mswdclassification.MembershipSector.Safe().ToUpper())
            {
                case "ARTISANAL FISHER FOLK":
                    var artisenalfisherfolk = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Artisenalcheckbox']");
                    if (artisenalfisherfolk != null)
                    {
                        string existingStyle = artisenalfisherfolk.GetAttributeValue("style", "");
                        artisenalfisherfolk.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "FARMER AND LANDLESS RURAL WORKER":
                    var farmerandlandless = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Farmercheckbox']");
                    if (farmerandlandless != null)
                    {
                        string existingStyle = farmerandlandless.GetAttributeValue("style", "");
                        farmerandlandless.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "URBAN POOR":
                    var urbanpoor = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Urbancheckbox']");
                    if (urbanpoor != null)
                    {
                        string existingStyle = urbanpoor.GetAttributeValue("style", "");
                        urbanpoor.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "INDIGENOUS PEOPLES":
                    var indigenouspeople = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Indigenouscheckbox']");
                    if (indigenouspeople != null)
                    {
                        string existingStyle = indigenouspeople.GetAttributeValue("style", "");
                        indigenouspeople.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "SENIOR CITIZEN":
                    var seniorcitizen = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Seniorcitizencheckbox']");
                    if (seniorcitizen != null)
                    {
                        string existingStyle = seniorcitizen.GetAttributeValue("style", "");
                        seniorcitizen.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "FORMAL LABOR AND MIGRANT WORKERS":
                    var formallabor = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Formallaborcheckbox']");
                    if (formallabor != null)
                    {
                        string existingStyle = formallabor.GetAttributeValue("style", "");
                        formallabor.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "WORKERS IN INFORMAL SECTOR":
                    var informalworker = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Workersinformalcheckbox']");
                    if (informalworker != null)
                    {
                        string existingStyle = informalworker.GetAttributeValue("style", "");
                        informalworker.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "PWD":
                    var pwd = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Pwdcheckbox']");
                    if (pwd != null)
                    {
                        string existingStyle = pwd.GetAttributeValue("style", "");
                        pwd.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "VICTIMS OF DISASTER AND CALAMITY":
                    var victimsofdisaster = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Victimscheckbox']");
                    if (victimsofdisaster != null)
                    {
                        string existingStyle = victimsofdisaster.GetAttributeValue("style", "");
                        victimsofdisaster.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                default:
                    var others = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Otherscheckbox']");
                    if (others != null)
                    {
                        string existingStyle = others.GetAttributeValue("style", "");
                        others.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    var otherstext = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Others']");
                    if (otherstext != null)
                    {
                        otherstext.InnerHtml = mswdclassification.MembershipSector;
                    }
                    break;

            }

            return htmlDoc.DocumentNode.OuterHtml; // Return updated HTML
        }

        public async Task<string> ModifyHtmlTemplateAsync_Page2(string htmlContent, int id, int assessmentID)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using var context = new ApplicationDbContext(connectionString);

            var patient = await context.Patients.FindAsync(id);
            var monthlyexpenses = await context.MonthlyExpenses.FirstOrDefaultAsync(m => m.AssessmentID == assessmentID && m.PatientID == id);
            var utilities = await context.Utilities.FirstOrDefaultAsync(u => u.AssessmentID == assessmentID && u.PatientID == id);
            var medicalhistory = await context.MedicalHistory.FirstOrDefaultAsync(m => m.AssessmentID == assessmentID && m.PatientID == id);
            var childhealth = await context.ChildHealth.FirstOrDefaultAsync(c => c.AssessmentID == assessmentID && c.PatientID == id);

            if (patient == null)
            {
                return string.Empty;
            }

            // USING HTMLAGILITYPACK
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // MONTHLY EXPENSES
            htmlDoc.SetInnerHtml("//div[@class='Houseandlot']", monthlyexpenses.HouseAndLot.Safe());
            htmlDoc.SetInnerHtml("//div[@class='Foodandwater']", monthlyexpenses.FoodAndWater.Safe());
            htmlDoc.SetInnerHtml("//div[@class='Education']", monthlyexpenses.Education.Safe());
            htmlDoc.SetInnerHtml("//div[@class='Clothing']", monthlyexpenses.Clothing.Safe());
            htmlDoc.SetInnerHtml("//div[@class='Communication']", monthlyexpenses.Communication.Safe());
            htmlDoc.SetInnerHtml("//div[@class='Househelp']", monthlyexpenses.HouseHelp.Safe());
            htmlDoc.SetInnerHtml("//div[@class='Medicalexpenses']", monthlyexpenses.MedicalExpenses.Safe());

            var others = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Others']");
            if (others != null)
            {
                if (monthlyexpenses.Others != null || monthlyexpenses.OthersAmount != 0)
                {
                    others.InnerHtml = $"{monthlyexpenses.Others.Safe()} {monthlyexpenses.OthersAmount.Safe()}";
                }
                else
                {
                    others.InnerHtml = "";
                }
            }

            htmlDoc.SetInnerHtml("//div[@class='Transportation']", monthlyexpenses.Transportation.Safe());
            htmlDoc.SetInnerHtml("//div[@class='Total']", monthlyexpenses.Total.Safe());

            // UTILITIES
            switch (utilities.LightSource.Safe().ToUpper())
            {
                case "ELECTRIC":
                    var electriccheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Electriccheckbox']");
                    if (electriccheckbox != null)
                    {
                        string existingStyle = electriccheckbox.GetAttributeValue("style", "");
                        electriccheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    var electrictext = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Electrictext']");
                    if (electrictext != null)
                    {
                        electrictext.InnerHtml = utilities.LightSourceAmount.Safe();
                    }
                    break;
                case "KEROSENE":
                    var kerosenecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Kerosenecheckbox']");
                    if (kerosenecheckbox != null)
                    {
                        string existingStyle = kerosenecheckbox.GetAttributeValue("style", "");
                        kerosenecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    var kerosenetext = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Kerosenetext']");
                    if (kerosenetext != null)
                    {
                        kerosenetext.InnerHtml = utilities.LightSourceAmount.Safe();
                    }
                    break;
                case "CANDLE":
                    var candlecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Candlecheckbox']");
                    if (candlecheckbox != null)
                    {
                        string existingStyle = candlecheckbox.GetAttributeValue("style", "");
                        candlecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    var candletext = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Candletext']");
                    if (candletext != null)
                    {
                        candletext.InnerHtml = utilities.LightSourceAmount.Safe();
                    }
                    break;
            }

            switch (utilities.FuelSource.Safe().ToUpper())
            {
                case "GAS":
                    var gascheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Gascheckbox']");
                    if (gascheckbox != null)
                    {
                        string existingStyle = gascheckbox.GetAttributeValue("style", "");
                        gascheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    var gastext = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Gastext']");
                    if (gastext != null)
                    {
                        gastext.InnerHtml = utilities.FuelSourceAmount.Safe();
                    }
                    break;
                case "FIREWOOD":
                    var firewoodcheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Firewoodcheckbox']");
                    if (firewoodcheckbox != null)
                    {
                        string existingStyle = firewoodcheckbox.GetAttributeValue("style", "");
                        firewoodcheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    var firewoodtext = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Firewoodtext']");
                    if (firewoodtext != null)
                    {
                        firewoodtext.InnerHtml = utilities.FuelSourceAmount.Safe();
                    }
                    break;
                case "CHARCOAL":
                    var charcoalcheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Charcoalcheckbox']");
                    if (charcoalcheckbox != null)
                    {
                        string existingStyle = charcoalcheckbox.GetAttributeValue("style", "");
                        charcoalcheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    var charcoaltext = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Charcoaltext']");
                    if (charcoaltext != null)
                    {
                        charcoaltext.InnerHtml = utilities.FuelSourceAmount.Safe();
                    }
                    break;
            }

            if (utilities.WaterSource.Safe().ToUpper() == "ARTESIAN WELL")
            {
                var artesianwell = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='ArtesianWell']");
                if (artesianwell != null)
                {
                    string existingStyle = artesianwell.GetAttributeValue("style", "");
                    artesianwell.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                }
            }
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Publiccheckbox", utilities.WaterSource.Safe().ToUpper() == "PUBLIC");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Privatecheckbox", utilities.WaterSource.Safe().ToUpper() == "PRIVATE");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Waterdistrictcheckbox", utilities.WaterSource.Safe().ToUpper() == "WATER DISTRICT");

            // MEDICAL HISTORY
            htmlDoc.SetInnerHtml("//div[@class='Admittingdiagnosis']", medicalhistory.AdmittingDiagnosis);
            htmlDoc.SetInnerHtml("//div[@class='Finaldiagnosisupondischarge']", medicalhistory.FinalDiagnosis);
            htmlDoc.SetInnerHtml("//div[@class='Durationofthesymptoms']", medicalhistory.DurationSymptomsPriorAdmission);
            htmlDoc.SetInnerHtml("//div[@class='Previoustreatment']", medicalhistory.PreviousTreatmentDuration);
            htmlDoc.SetInnerHtml("//div[@class='Treatmentplan']", medicalhistory.TreatmentPlan);
            htmlDoc.SetInnerHtml("//div[@class='Healthaccessibilityproblem']", medicalhistory.HealthAccessibilityProblems);

            // CHILD HEALTH
            htmlDoc.SetInnerHtml("//div[@class='Num1']", childhealth.OverallHealth);

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yesnum2checkbox", childhealth.HasHealthIssues == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nonum2checkbox", childhealth.HasHealthIssues == false);
            
            htmlDoc.SetInnerHtml("//div[@class='Num2']", childhealth.DescribeHealthIssues);

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yesnum3checkbox", childhealth.HasRecurrentConditions == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nonum3checkbox", childhealth.HasRecurrentConditions == false);

            htmlDoc.SetInnerHtml("//div[@class='Num3']", childhealth.DescribeRecurrentConditions);

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yesnum4checkbox", childhealth.HasEarTubes == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nonum4checkbox", childhealth.HasEarTubes == false);
            
            return htmlDoc.DocumentNode.OuterHtml; // Return updated HTML
        }

        public async Task<string> ModifyHtmlTemplateAsync_Page3(string htmlContent, int id, int assessmentID)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using var context = new ApplicationDbContext(connectionString);

            var patient = await context.Patients.FindAsync(id);
            var diagnoses = await context.Diagnoses.Where(p => p.AssessmentID == assessmentID && p.PatientID == id).ToListAsync();
            var medications = await context.Medications.Where(p => p.AssessmentID == assessmentID && p.PatientID == id).ToListAsync();
            var hospitalizationhistory = await context.HospitalizationHistory.Where(p => p.AssessmentID == assessmentID && p.PatientID == id).ToListAsync();
            var medicalscreenings = await context.MedicalScreenings.FirstOrDefaultAsync(c => c.AssessmentID == assessmentID && c.PatientID == id);
            var primarycaredoctor = await context.PrimaryCareDoctor.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID && p.PatientID == id);
            var presentingproblems = await context.PresentingProblems.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID && p.PatientID == id);

            if (patient == null)
            {
                return string.Empty;
            }

            // USING HTMLAGILITYPACK
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // DIAGNOSES
            int i = 1;
            foreach (var diagnosis in diagnoses)
            {
                htmlDoc.SetInnerHtml($"//div[@class='Medicalconditions{i}']", diagnosis.MedicalCondition);
                htmlDoc.SetInnerHtml($"//div[@class='Currentlyreceivingtreatment{i}']", diagnosis.ReceivingTreatment.Safe());
                htmlDoc.SetInnerHtml($"//div[@class='Provider{i}']", diagnosis.TreatmentProvider);
                htmlDoc.SetInnerHtml($"//div[@class='Doesthisconditioncausestress{i}']", diagnosis.DoesCauseStressOrImpairment.Safe());
                htmlDoc.SetInnerHtml($"//div[@class='Whathaveyoufound{i}']", diagnosis.TreatmentHelp);

                i++;
            }

            // MEDICATIONS
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yesnum6checkbox", medications[0].DoesTakeAnyMedication == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nonum6checkbox", medications[0].DoesTakeAnyMedication == false);
            
            if (medications[0].DoesTakeAnyMedication == true)
            {
                int medCount = 1;
                foreach (var medication in medications)
                {
                    htmlDoc.SetInnerHtml($"//div[@class='Medication{medCount}']", medication.Medication);
                    htmlDoc.SetInnerHtml($"//div[@class='Dosage{medCount}']", medication.Dosage);
                    htmlDoc.SetInnerHtml($"//div[@class='Frequency{medCount}']", medication.Frequency);
                    htmlDoc.SetInnerHtml($"//div[@class='Prescribedby{medCount}']", medication.PrescribedBy);
                    htmlDoc.SetInnerHtml($"//div[@class='Reasonformedication{medCount}']", medication.ReasonForMedication);

                    medCount++;
                }
            }

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yestakingmedicationcheckbox", medications[0].IsTakingMedicationAsPrescribed == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Notakingmedicationcheckbox", medications[0].IsTakingMedicationAsPrescribed == false);
            
            htmlDoc.SetInnerHtml("//div[@class='Isyourchildtakingmedication']", medications[0].DescribeTakingMedication);
            htmlDoc.SetInnerHtml("//div[@class='Additionalinformation']", medications[0].AdditionalInfo);

            // HOSPITALIZATION HISTORY
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yesnum7checkbox", hospitalizationhistory[0].HasSeriousAccidentOrIllness == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nonum7checkbox", hospitalizationhistory[0].HasSeriousAccidentOrIllness == false);
            
            if (hospitalizationhistory[0].HasSeriousAccidentOrIllness == true)
            {
                int hospCount = 1;
                foreach (var hospital in hospitalizationhistory)
                {
                    htmlDoc.SetInnerHtml($"//div[@class='Reasonforprevious{hospCount}']", hospital.Reason);
                    htmlDoc.SetInnerHtml($"//div[@class='Datelocationofhospitalization{hospCount}']", hospital.Location);

                    hospCount++;
                }
            }

            // MEDICAL SCREENINGS
            htmlDoc.SetInnerHtml("//div[@class='Hasscreeningtest']", medicalscreenings.HasScreeningDone.Safe());
           
            if (medicalscreenings.HasScreeningDone)
            {
                htmlDoc.SetInnerHtml("//div[@class='Hearingdate']", medicalscreenings.HearingTestDate.Safe());
                htmlDoc.SetInnerHtml("//div[@class='Hearingoutcome']", medicalscreenings.HearingTestOutcome);
                htmlDoc.SetInnerHtml("//div[@class='Visiondate']", medicalscreenings.VisionTestDate.Safe());
                htmlDoc.SetInnerHtml("//div[@class='Visionoutcome']", medicalscreenings.VisionTestOutcome);
                htmlDoc.SetInnerHtml("//div[@class='Speechdate']", medicalscreenings.SpeechTestDate.Safe());
                htmlDoc.SetInnerHtml("//div[@class='Speechoutcome']", medicalscreenings.SpeechTestOutcome);

                // PRIMARY CARE DOCTOR
                htmlDoc.SetInnerHtml("//div[@class='Primarycaredoctor']", primarycaredoctor.DoctorName);
                htmlDoc.SetInnerHtml("//div[@class='Facility']", primarycaredoctor.Facility);
                htmlDoc.SetInnerHtml("//div[@class='Phonenumber']", primarycaredoctor.PhoneNumber.Safe() == "0" ? "" : primarycaredoctor.PhoneNumber.Safe());
            }

            // PRESENTING PROBLEMS
            htmlDoc.SetInnerHtml("//div[@class='Presentingproblem']", presentingproblems.PresentingProblem);

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Onecheckbox", presentingproblems.Severity == 1);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Twocheckbox", presentingproblems.Severity == 2);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Threecheckbox", presentingproblems.Severity == 3);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Fourcheckbox", presentingproblems.Severity == 4);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Fivecheckbox", presentingproblems.Severity == 5);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Sixcheckbox", presentingproblems.Severity == 6);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Sevencheckbox", presentingproblems.Severity == 7);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Eightcheckbox", presentingproblems.Severity == 8);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Ninecheckbox", presentingproblems.Severity == 9);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Tencheckbox", presentingproblems.Severity == 10);
            
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Sleepingmorecheckbox", presentingproblems.ChangeInSleepPattern.Safe().ToUpper() == "SLEEPING MORE");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Sleepinglesscheckbox", presentingproblems.ChangeInSleepPattern.Safe().ToUpper() == "SLEEPING LESS");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Difficultyfallingasleepcheckbox", presentingproblems.ChangeInSleepPattern.Safe().ToUpper() == "DIFFICULTY FALLING ASLEEP");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Difficultystayingasleepcheckbox", presentingproblems.ChangeInSleepPattern.Safe().ToUpper() == "DIFFICULTY STAYING ASLEEP");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Difficultywakingupcheckbox", presentingproblems.ChangeInSleepPattern.Safe().ToUpper() == "DIFFICULTY WAKING UP");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Difficultystayingawakecheckbox", presentingproblems.ChangeInSleepPattern.Safe().ToUpper() == "DIFFICULTY STAYING AWAKE");
            
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Decreasedconcentrationcheckbox", presentingproblems.Concentration.Safe().ToUpper() == "DECREASED CONCENTRATION");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Increasedconcentrationcheckbox", presentingproblems.Concentration.Safe().ToUpper() == "INCREASED OR EXCESSIVE CONCENTRATION");
            
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Increasedappetitecheckbox", presentingproblems.ChangeInAppetite.Safe().ToUpper() == "INCREASED APPETITE");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Decreasedappetitecheckbox", presentingproblems.ChangeInAppetite.Safe().ToUpper() == "DECREASED APPETITE");
            
            htmlDoc.SetInnerHtml("//div[@class='Increasedanxiety']", presentingproblems.IncreasedAnxiety);
            htmlDoc.SetInnerHtml("//div[@class='Moodswings']", presentingproblems.MoodSwings);
            htmlDoc.SetInnerHtml("//div[@class='Behavioralproblems']", presentingproblems.BehavioralChanges);

            switch (presentingproblems.Victimization.Safe().ToUpper())
            {
                case "PHYSICAL ABUSE":
                    var physicalabuse = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Physicalabusecheckbox']");
                    if (physicalabuse != null)
                    {
                        string existingStyle = physicalabuse.GetAttributeValue("style", "");
                        physicalabuse.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "SEXUAL ABUSE":
                    var sexualabuse = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Sexualabusecheckbox']");
                    if (sexualabuse != null)
                    {
                        string existingStyle = sexualabuse.GetAttributeValue("style", "");
                        sexualabuse.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "PSYCHOLOGICAL ABUSE":
                    var psychologicalabuse = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Psychologicalabusecheckbox']");
                    if (psychologicalabuse != null)
                    {
                        string existingStyle = psychologicalabuse.GetAttributeValue("style", "");
                        psychologicalabuse.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "ROBBERY VICTIM":
                    var robberyvictim = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Robberyvictimcheckbox']");
                    if (robberyvictim != null)
                    {
                        string existingStyle = robberyvictim.GetAttributeValue("style", "");
                        robberyvictim.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "ASSAULT VICTIM":
                    var assaultvictim = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Assaultvictimcheckbox']");
                    if (assaultvictim != null)
                    {
                        string existingStyle = assaultvictim.GetAttributeValue("style", "");
                        assaultvictim.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "DATING VIOLENCE":
                    var datingviolence = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Datingviolencecheckbox']");
                    if (datingviolence != null)
                    {
                        string existingStyle = datingviolence.GetAttributeValue("style", "");
                        datingviolence.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "DOMESTIC VIOLENCE":
                    var domesticviolence = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Domesticviolencecheckbox']");
                    if (domesticviolence != null)
                    {
                        string existingStyle = domesticviolence.GetAttributeValue("style", "");
                        domesticviolence.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "HUMAN TRAFFICKING":
                    var humantrafficking = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Humantraffickingcheckbox']");
                    if (humantrafficking != null)
                    {
                        string existingStyle = humantrafficking.GetAttributeValue("style", "");
                        humantrafficking.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "DUI/DWI CRASH":
                    var dui = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Duicrashcheckbox']");
                    if (dui != null)
                    {
                        string existingStyle = dui.GetAttributeValue("style", "");
                        dui.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "SURVIVORS OF HOMICIDE VICTIMS":
                    var survivors = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Survivorscheckbox']");
                    if (survivors != null)
                    {
                        string existingStyle = survivors.GetAttributeValue("style", "");
                        survivors.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "N/A":
                    break;
                default:
                    var others = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Othercheckbox']");
                    if (others != null)
                    {
                        string existingStyle = others.GetAttributeValue("style", "");
                        others.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    var otherstext = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Other']");
                    if (otherstext != null)
                    {
                        otherstext.InnerHtml = presentingproblems.Victimization;
                    }
                    break;

            }

            htmlDoc.SetInnerHtml("//div[@class='Otherconcerns']", presentingproblems.DescribeOtherConcern);

            return htmlDoc.DocumentNode.OuterHtml; // Return updated HTML
        }

        public async Task<string> ModifyHtmlTemplateAsync_Page4(string htmlContent, int id, int assessmentID)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using var context = new ApplicationDbContext(connectionString);

            var patient = await context.Patients.FindAsync(id);
            var presentingproblems = await context.PresentingProblems.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID && p.PatientID == id);
            var recentlosses = await context.RecentLosses.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID && p.PatientID == id);
            var pregnancybirthhistory = await context.PregnancyBirthHistory.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID && p.PatientID == id);
            var developmentalhistory = await context.DevelopmentalHistory.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID && p.PatientID == id);
            var mentalhealthhistory = await context.MentalHealthHistory.Where(p => p.AssessmentID == assessmentID && p.PatientID == id).ToListAsync();
            var familyhistory = await context.FamilyHistory.Where(p => p.AssessmentID == assessmentID && p.PatientID == id).ToListAsync();
            var safetyconcerns = await context.SafetyConcerns.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID && p.PatientID == id);

            if (patient == null)
            {
                return string.Empty;
            }

            // USING HTMLAGILITYPACK
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // PRESENTING PROBLEMS
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Oneweekcheckbox", presentingproblems.DurationOfStress.Safe().ToUpper() == "ONE WEEK");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Onemonthcheckbox", presentingproblems.DurationOfStress.Safe().ToUpper() == "ONE MONTH");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Onetosixmonthscheckbox", presentingproblems.DurationOfStress.Safe().ToUpper() == "1-6 MONTHS");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Sixmonthstooneyearcheckbox", presentingproblems.DurationOfStress.Safe().ToUpper() == "6 MONTHS – 1 YEAR");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Longerthanoneyearcheckbox", presentingproblems.DurationOfStress.Safe().ToUpper() == "LONGER THAN ONE YEAR");
            
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Onecheckbox", presentingproblems.CopingLevel == 1);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Twocheckbox", presentingproblems.CopingLevel == 2);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Threecheckbox", presentingproblems.CopingLevel == 3);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Fourcheckbox", presentingproblems.CopingLevel == 4);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Fivecheckbox", presentingproblems.CopingLevel == 5);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Sixcheckbox", presentingproblems.CopingLevel == 6);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Sevencheckbox", presentingproblems.CopingLevel == 7);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Eightcheckbox", presentingproblems.CopingLevel == 8);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Ninecheckbox", presentingproblems.CopingLevel == 9);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Tencheckbox", presentingproblems.CopingLevel == 10);
            
            htmlDoc.SetInnerHtml("//div[@class='Otherfamilysituation']", presentingproblems.OtherFamilySituation);

            // RECENT LOSSES
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Familymembercheckbox", recentlosses.FamilyMemberLoss);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Friendcheckbox", recentlosses.FriendLoss);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Healthcheckbox", recentlosses.HealthLoss);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Lifestylecheckbox", recentlosses.LifestyleLoss);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Jobcheckbox", recentlosses.JobLoss);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Incomecheckbox", recentlosses.IncomeLoss);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Housingcheckbox", recentlosses.HousingLoss);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nonecheckbox", recentlosses.NoneLoss);
            
            htmlDoc.SetInnerHtml("//div[@class='Whotext']", recentlosses.Name);
            htmlDoc.SetInnerHtml("//div[@class='Whentext']", recentlosses.Date.Safe());
            htmlDoc.SetInnerHtml("//div[@class='Naturetext']", recentlosses.NatureOfLoss);
            htmlDoc.SetInnerHtml("//div[@class='Otherlossestext']", recentlosses.OtherLosses);
            htmlDoc.SetInnerHtml("//div[@class='Additionalinforecentlossestext']", recentlosses.AdditionalInfo);

            // PREGNANCY BIRTH HISTORY
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yesonecheckbox", pregnancybirthhistory.HasPregnancyComplications == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Noonecheckbox", pregnancybirthhistory.HasPregnancyComplications == false);
            
            htmlDoc.SetInnerHtml("//div[@class='Describepregnancycomplications']", pregnancybirthhistory.DescribePregnancyComplications);

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Fulltermbirthcheckbox", pregnancybirthhistory.IsFullTermBirth == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Prematurebirthcheckbox", pregnancybirthhistory.IsFullTermBirth == false);
            
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yestwocheckbox", pregnancybirthhistory.HasBirthComplications == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Notwocheckbox", pregnancybirthhistory.HasBirthComplications == false);
            
            htmlDoc.SetInnerHtml("//div[@class='Describebirthcomplications']", pregnancybirthhistory.DescribeBirthComplications);

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yesthreecheckbox", pregnancybirthhistory.HasConsumedDrugs == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nothreecheckbox", pregnancybirthhistory.HasConsumedDrugs == false);
            
            var birthweightlbs = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Birthweightlbs']");
            if (birthweightlbs != null)
            {
                birthweightlbs.InnerHtml = pregnancybirthhistory.BirthWeightLbs.ToString("0.##").Safe();
            }

            var birthweightoz = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Birthweightoz']");
            if (birthweightoz != null)
            {
                birthweightoz.InnerHtml = pregnancybirthhistory.BirthWeightOz.ToString("0.##").Safe();
            }

            htmlDoc.SetInnerHtml("//div[@class='Childhealthatbirthtext']", pregnancybirthhistory.BirthHealth);
            htmlDoc.SetInnerHtml("//div[@class='Lengthofhospitalstaytext']", pregnancybirthhistory.LengthOfHospitalStay);

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yesfivecheckbox", pregnancybirthhistory.PostpartumDepression == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nofivecheckbox", pregnancybirthhistory.PostpartumDepression == false);
            
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yessixcheckbox", pregnancybirthhistory.WasChildAdopted == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nosixcheckbox", pregnancybirthhistory.WasChildAdopted == false);
            
            if (pregnancybirthhistory.WasChildAdopted)
            {
                htmlDoc.SetInnerHtml("//div[@class='Adoptedagetext']", pregnancybirthhistory.ChildAdoptedAge);

                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Domesticadoptioncheckbox", pregnancybirthhistory.AdoptionType.Safe().ToUpper() == "DOMESTIC ADOPTION");
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Internationaladoptioncheckbox", pregnancybirthhistory.AdoptionType.Safe().ToUpper() == "INTERNATIONAL ADOPTION");
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Undocumentedadoptioncheckbox", pregnancybirthhistory.AdoptionType.Safe().ToUpper() == "UNDOCUMENTED ADOPTION");
                
                htmlDoc.SetInnerHtml("//div[@class='Adoptioncountrytext']", pregnancybirthhistory.AdoptionCountry);
            }

            // DEVELOPMENTAL HISTORY
            htmlDoc.SetInnerHtml("//div[@class='Rolledovertext']", developmentalhistory.RolledOverAge);
            htmlDoc.SetInnerHtml("//div[@class='Crawledtext']", developmentalhistory.CrawledAge);
            htmlDoc.SetInnerHtml("//div[@class='Walkedtext']", developmentalhistory.WalkedAge);
            htmlDoc.SetInnerHtml("//div[@class='Talkedtext']", developmentalhistory.TalkedAge);
            htmlDoc.SetInnerHtml("//div[@class='Toilettrainedtext']", developmentalhistory.ToiletTrainedAge);

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Speechcheckbox", developmentalhistory.SpeechConcerns);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Motorskillscheckbox", developmentalhistory.MotorSkillsConcerns);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Cognitivecheckbox", developmentalhistory.CognitiveConcerns);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Sensorycheckbox", developmentalhistory.SensoryConcerns);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Behavioralcheckbox", developmentalhistory.BehavioralConcerns);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Emotionalcheckbox", developmentalhistory.EmotionalConcerns);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Socialcheckbox", developmentalhistory.SocialConcerns);
           
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yessignificantissuecheckbox", developmentalhistory.HasSignificantDisturbance == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nosignificantissuecheckbox", developmentalhistory.HasSignificantDisturbance == false);
            
            htmlDoc.SetInnerHtml("//div[@class='Describesignificantissue']", developmentalhistory.DescribeSignificantDisturbance);

            // MENTAL HEALTH HISTORY
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yesreceivedcounselingcheckbox", mentalhealthhistory[0].HasReceivedCounseling == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Noreceivedcounselingcheckbox", mentalhealthhistory[0].HasReceivedCounseling == false);
            
            if (mentalhealthhistory[0].HasReceivedCounseling)
            {
                int mentalCount = 1;
                foreach (var mentalhealth in mentalhealthhistory)
                {
                    htmlDoc.SetInnerHtml($"//div[@class='Dateofservice{mentalCount}']", mentalhealth.DateOfService.Safe());
                    htmlDoc.SetInnerHtml($"//div[@class='Placeprovider{mentalCount}']", mentalhealth.Provider);
                    htmlDoc.SetInnerHtml($"//div[@class='Reasonfortreatment{mentalCount}']", mentalhealth.ReasonForTreatment);
                    htmlDoc.SetInnerHtml($"//div[@class='Weretheserviceshelpful{mentalCount}']", mentalhealth.WereServicesHelpful.Safe());

                    mentalCount++;
                }
            }

            // FAMILY HISTORY
            foreach (var family in familyhistory)
            {
                int familyCount = family.IsSelf ? 1 : 2;

                htmlDoc.SetInnerHtml($"//div[@class='Depression{familyCount}']", family.HasDepression.Safe());
                htmlDoc.SetInnerHtml($"//div[@class='Anxiety{familyCount}']", family.HasAnxiety.Safe());
                htmlDoc.SetInnerHtml($"//div[@class='Bipolardisorder{familyCount}']", family.HasBipolarDisorder.Safe());
                htmlDoc.SetInnerHtml($"//div[@class='Schizophrenia{familyCount}']", family.HasSchizophrenia.Safe());
                htmlDoc.SetInnerHtml($"//div[@class='Adhdadd{familyCount}']", family.HasADHD_ADD.Safe());
                htmlDoc.SetInnerHtml($"//div[@class='Traumahistory{familyCount}']", family.HasTraumaHistory.Safe());
                htmlDoc.SetInnerHtml($"//div[@class='Abusivebehavior{familyCount}']", family.HasAbusiveBehavior.Safe());
                htmlDoc.SetInnerHtml($"//div[@class='Alcoholabuse{familyCount}']", family.HasAlcoholAbuse.Safe());
                htmlDoc.SetInnerHtml($"//div[@class='Drugabuse{familyCount}']", family.HasDrugAbuse.Safe());
                htmlDoc.SetInnerHtml($"//div[@class='Incarceration{familyCount}']", family.HasIncarceration.Safe());

            }

            htmlDoc.SetInnerHtml($"//div[@class='Additionalinfopatientfamilytext']", familyhistory[0].AdditionalInfo);

            // SAFETY CONCERNS
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "yespresentlysuicidalcheckbox", safetyconcerns.IsSuicidal == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "nopresentlysuicidalcheckbox", safetyconcerns.IsSuicidal == false);
            
            htmlDoc.SetInnerHtml("//div[@class='ifyesexplainpresentlysuicidal']", safetyconcerns.DescribeSuicidal);

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "yeshasattemptedsuicidecheckbox", safetyconcerns.HasAttemptedSuicide == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "nohasattemptedsuicidecheckbox", safetyconcerns.HasAttemptedSuicide == false);
            
            htmlDoc.SetInnerHtml("//div[@class='ifyesexplainattemptedsuicide']", safetyconcerns.DescribeAttemptedSuicide);

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "yessuicidehistorycheckbox", safetyconcerns.IsThereHistoryOfSuicide == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "nosuicidehistorycheckbox", safetyconcerns.IsThereHistoryOfSuicide == false);
            
            htmlDoc.SetInnerHtml("//div[@class='ifyesexplainsuicidehistory']", safetyconcerns.DescribeHistoryOfSuicide);

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "yesinflictedburnscheckbox", safetyconcerns.HasSelfHarm == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "noinflictedburnscheckbox", safetyconcerns.HasSelfHarm == false);
           
            return htmlDoc.DocumentNode.OuterHtml;
        }

        public async Task<string> ModifyHtmlTemplateAsync_Page5(string htmlContent, int id, int assessmentID)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using var context = new ApplicationDbContext(connectionString);

            var patient = await context.Patients.FindAsync(id);
            var safetyconcerns = await context.SafetyConcerns.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID && p.PatientID == id);
            var currentfunctioning = await context.CurrentFunctioning.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID && p.PatientID == id);
            var parentchildrelationship = await context.ParentChildRelationship.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID && p.PatientID == id);

            if (patient == null)
            {
                return string.Empty;
            }

            // USING HTMLAGILITYPACK
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // SAFETY CONCERNS
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "yespresentlyhomicidalcheckbox", safetyconcerns.IsHomicidal == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "nopresentlyhomicidalcheckbox", safetyconcerns.IsHomicidal == false);
            
            htmlDoc.SetInnerHtml("//div[@class='ifyesexplainpresentlyhomicidal']", safetyconcerns.DescribeHomicidal);
            htmlDoc.SetInnerHtml("//div[@class='safetyconcernsadditionalinfo']", safetyconcerns.AdditionalInfo);

            // CURRENT FUNCTIONING
            htmlDoc.SetInnerHtml("//div[@class='describeconcernscurrentfunctioning']", currentfunctioning.DescribeConcerns);

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "eatingcheckbox", currentfunctioning.EatingConcerns);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "hygienegroomingcheckbox", currentfunctioning.HygieneConcerns);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "sleepingcheckbox", currentfunctioning.SleepingConcerns);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "activitiesplaycheckbox", currentfunctioning.ActivitiesConcerns);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "socialrelationshipscheckbox", currentfunctioning.SocialRelationshipsConcerns);
            
            switch (currentfunctioning.EnergyLevel)
            {
                case 1:
                    var one = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='oneA']");
                    if (one != null)
                    {
                        string existingStyle = one.GetAttributeValue("style", "");
                        one.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 2:
                    var two = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='twoA']");
                    if (two != null)
                    {
                        string existingStyle = two.GetAttributeValue("style", "");
                        two.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 3:
                    var three = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='threeA']");
                    if (three != null)
                    {
                        string existingStyle = three.GetAttributeValue("style", "");
                        three.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 4:
                    var four = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='fourA']");
                    if (four != null)
                    {
                        string existingStyle = four.GetAttributeValue("style", "");
                        four.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 5:
                    var five = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='fiveA']");
                    if (five != null)
                    {
                        string existingStyle = five.GetAttributeValue("style", "");
                        five.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 6:
                    var six = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sixA']");
                    if (six != null)
                    {
                        string existingStyle = six.GetAttributeValue("style", "");
                        six.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 7:
                    var seven = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sevenA']");
                    if (seven != null)
                    {
                        string existingStyle = seven.GetAttributeValue("style", "");
                        seven.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;

            }

            switch (currentfunctioning.PhysicalLevel)
            {
                case 1:
                    var one = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='oneB']");
                    if (one != null)
                    {
                        string existingStyle = one.GetAttributeValue("style", "");
                        one.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 2:
                    var two = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='twoB']");
                    if (two != null)
                    {
                        string existingStyle = two.GetAttributeValue("style", "");
                        two.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 3:
                    var three = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='threeB']");
                    if (three != null)
                    {
                        string existingStyle = three.GetAttributeValue("style", "");
                        three.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 4:
                    var four = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='fourB']");
                    if (four != null)
                    {
                        string existingStyle = four.GetAttributeValue("style", "");
                        four.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 5:
                    var five = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='fiveB']");
                    if (five != null)
                    {
                        string existingStyle = five.GetAttributeValue("style", "");
                        five.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 6:
                    var six = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sixB']");
                    if (six != null)
                    {
                        string existingStyle = six.GetAttributeValue("style", "");
                        six.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 7:
                    var seven = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sevenB']");
                    if (seven != null)
                    {
                        string existingStyle = seven.GetAttributeValue("style", "");
                        seven.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
            }

            switch (currentfunctioning.AnxiousLevel)
            {
                case 1:
                    var one = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='oneC1']");
                    if (one != null)
                    {
                        string existingStyle = one.GetAttributeValue("style", "");
                        one.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 2:
                    var two = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='twoC1']");
                    if (two != null)
                    {
                        string existingStyle = two.GetAttributeValue("style", "");
                        two.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 3:
                    var three = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='threeC1']");
                    if (three != null)
                    {
                        string existingStyle = three.GetAttributeValue("style", "");
                        three.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 4:
                    var four = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='fourC1']");
                    if (four != null)
                    {
                        string existingStyle = four.GetAttributeValue("style", "");
                        four.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 5:
                    var five = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='fiveC1']");
                    if (five != null)
                    {
                        string existingStyle = five.GetAttributeValue("style", "");
                        five.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 6:
                    var six = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sixC1']");
                    if (six != null)
                    {
                        string existingStyle = six.GetAttributeValue("style", "");
                        six.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 7:
                    var seven = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sevenC1']");
                    if (seven != null)
                    {
                        string existingStyle = seven.GetAttributeValue("style", "");
                        seven.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
            }

            switch (currentfunctioning.HappyLevel)
            {
                case 1:
                    var one = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='oneC2']");
                    if (one != null)
                    {
                        string existingStyle = one.GetAttributeValue("style", "");
                        one.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 2:
                    var two = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='twoC2']");
                    if (two != null)
                    {
                        string existingStyle = two.GetAttributeValue("style", "");
                        two.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 3:
                    var three = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='threeC2']");
                    if (three != null)
                    {
                        string existingStyle = three.GetAttributeValue("style", "");
                        three.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 4:
                    var four = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='fourC2']");
                    if (four != null)
                    {
                        string existingStyle = four.GetAttributeValue("style", "");
                        four.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 5:
                    var five = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='fiveC2']");
                    if (five != null)
                    {
                        string existingStyle = five.GetAttributeValue("style", "");
                        five.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 6:
                    var six = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sixC2']");
                    if (six != null)
                    {
                        string existingStyle = six.GetAttributeValue("style", "");
                        six.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 7:
                    var seven = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sevenC2']");
                    if (seven != null)
                    {
                        string existingStyle = seven.GetAttributeValue("style", "");
                        seven.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
            }

            switch (currentfunctioning.CuriousLevel)
            {
                case 1:
                    var one = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='oneC3']");
                    if (one != null)
                    {
                        string existingStyle = one.GetAttributeValue("style", "");
                        one.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 2:
                    var two = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='twoC3']");
                    if (two != null)
                    {
                        string existingStyle = two.GetAttributeValue("style", "");
                        two.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 3:
                    var three = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='threeC3']");
                    if (three != null)
                    {
                        string existingStyle = three.GetAttributeValue("style", "");
                        three.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 4:
                    var four = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='fourC3']");
                    if (four != null)
                    {
                        string existingStyle = four.GetAttributeValue("style", "");
                        four.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 5:
                    var five = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='fiveC3']");
                    if (five != null)
                    {
                        string existingStyle = five.GetAttributeValue("style", "");
                        five.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 6:
                    var six = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sixC3']");
                    if (six != null)
                    {
                        string existingStyle = six.GetAttributeValue("style", "");
                        six.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 7:
                    var seven = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sevenC3']");
                    if (seven != null)
                    {
                        string existingStyle = seven.GetAttributeValue("style", "");
                        seven.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
            }

            switch (currentfunctioning.AngryLevel)
            {
                case 1:
                    var one = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='oneC4']");
                    if (one != null)
                    {
                        string existingStyle = one.GetAttributeValue("style", "");
                        one.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 2:
                    var two = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='twoC4']");
                    if (two != null)
                    {
                        string existingStyle = two.GetAttributeValue("style", "");
                        two.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 3:
                    var three = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='threeC4']");
                    if (three != null)
                    {
                        string existingStyle = three.GetAttributeValue("style", "");
                        three.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 4:
                    var four = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='fourC4']");
                    if (four != null)
                    {
                        string existingStyle = four.GetAttributeValue("style", "");
                        four.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 5:
                    var five = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='fiveC4']");
                    if (five != null)
                    {
                        string existingStyle = five.GetAttributeValue("style", "");
                        five.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 6:
                    var six = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sixC4']");
                    if (six != null)
                    {
                        string existingStyle = six.GetAttributeValue("style", "");
                        six.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 7:
                    var seven = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sevenC4']");
                    if (seven != null)
                    {
                        string existingStyle = seven.GetAttributeValue("style", "");
                        seven.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
            }

            switch (currentfunctioning.IntensityLevel)
            {
                case 1:
                    var one = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='oneD']");
                    if (one != null)
                    {
                        string existingStyle = one.GetAttributeValue("style", "");
                        one.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 2:
                    var two = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='twoD']");
                    if (two != null)
                    {
                        string existingStyle = two.GetAttributeValue("style", "");
                        two.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 3:
                    var three = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='threeD']");
                    if (three != null)
                    {
                        string existingStyle = three.GetAttributeValue("style", "");
                        three.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 4:
                    var four = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='fourD']");
                    if (four != null)
                    {
                        string existingStyle = four.GetAttributeValue("style", "");
                        four.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 5:
                    var five = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='fiveD']");
                    if (five != null)
                    {
                        string existingStyle = five.GetAttributeValue("style", "");
                        five.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 6:
                    var six = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sixD']");
                    if (six != null)
                    {
                        string existingStyle = six.GetAttributeValue("style", "");
                        six.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 7:
                    var seven = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sevenD']");
                    if (seven != null)
                    {
                        string existingStyle = seven.GetAttributeValue("style", "");
                        seven.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
            }

            switch (currentfunctioning.PersistenceLevel)
            {
                case 1:
                    var one = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='oneE']");
                    if (one != null)
                    {
                        string existingStyle = one.GetAttributeValue("style", "");
                        one.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 2:
                    var two = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='twoE']");
                    if (two != null)
                    {
                        string existingStyle = two.GetAttributeValue("style", "");
                        two.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 3:
                    var three = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='threeE']");
                    if (three != null)
                    {
                        string existingStyle = three.GetAttributeValue("style", "");
                        three.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 4:
                    var four = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='fourE']");
                    if (four != null)
                    {
                        string existingStyle = four.GetAttributeValue("style", "");
                        four.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 5:
                    var five = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='fiveE']");
                    if (five != null)
                    {
                        string existingStyle = five.GetAttributeValue("style", "");
                        five.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 6:
                    var six = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sixE']");
                    if (six != null)
                    {
                        string existingStyle = six.GetAttributeValue("style", "");
                        six.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 7:
                    var seven = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sevenE']");
                    if (seven != null)
                    {
                        string existingStyle = seven.GetAttributeValue("style", "");
                        seven.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
            }

            switch (currentfunctioning.SensitivityLevel)
            {
                case 1:
                    var one = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='oneF']");
                    if (one != null)
                    {
                        string existingStyle = one.GetAttributeValue("style", "");
                        one.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 2:
                    var two = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='twoF']");
                    if (two != null)
                    {
                        string existingStyle = two.GetAttributeValue("style", "");
                        two.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 3:
                    var three = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='threeF']");
                    if (three != null)
                    {
                        string existingStyle = three.GetAttributeValue("style", "");
                        three.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 4:
                    var four = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='fourF']");
                    if (four != null)
                    {
                        string existingStyle = four.GetAttributeValue("style", "");
                        four.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 5:
                    var five = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='fiveF']");
                    if (five != null)
                    {
                        string existingStyle = five.GetAttributeValue("style", "");
                        five.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 6:
                    var six = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sixF']");
                    if (six != null)
                    {
                        string existingStyle = six.GetAttributeValue("style", "");
                        six.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 7:
                    var seven = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sevenF']");
                    if (seven != null)
                    {
                        string existingStyle = seven.GetAttributeValue("style", "");
                        seven.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
            }

            switch (currentfunctioning.PerceptivenessLevel)
            {
                case 1:
                    var one = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='oneG']");
                    if (one != null)
                    {
                        string existingStyle = one.GetAttributeValue("style", "");
                        one.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 2:
                    var two = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='twoG']");
                    if (two != null)
                    {
                        string existingStyle = two.GetAttributeValue("style", "");
                        two.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 3:
                    var three = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='threeG']");
                    if (three != null)
                    {
                        string existingStyle = three.GetAttributeValue("style", "");
                        three.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 4:
                    var four = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='fourG']");
                    if (four != null)
                    {
                        string existingStyle = four.GetAttributeValue("style", "");
                        four.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 5:
                    var five = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='fiveG']");
                    if (five != null)
                    {
                        string existingStyle = five.GetAttributeValue("style", "");
                        five.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 6:
                    var six = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sixG']");
                    if (six != null)
                    {
                        string existingStyle = six.GetAttributeValue("style", "");
                        six.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 7:
                    var seven = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sevenG']");
                    if (seven != null)
                    {
                        string existingStyle = seven.GetAttributeValue("style", "");
                        seven.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
            }

            switch (currentfunctioning.AdaptabilityLevel)
            {
                case 1:
                    var one = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='oneH']");
                    if (one != null)
                    {
                        string existingStyle = one.GetAttributeValue("style", "");
                        one.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 2:
                    var two = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='twoH']");
                    if (two != null)
                    {
                        string existingStyle = two.GetAttributeValue("style", "");
                        two.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 3:
                    var three = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='threeH']");
                    if (three != null)
                    {
                        string existingStyle = three.GetAttributeValue("style", "");
                        three.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 4:
                    var four = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='fourH']");
                    if (four != null)
                    {
                        string existingStyle = four.GetAttributeValue("style", "");
                        four.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 5:
                    var five = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='fiveH']");
                    if (five != null)
                    {
                        string existingStyle = five.GetAttributeValue("style", "");
                        five.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 6:
                    var six = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sixH']");
                    if (six != null)
                    {
                        string existingStyle = six.GetAttributeValue("style", "");
                        six.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 7:
                    var seven = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sevenH']");
                    if (seven != null)
                    {
                        string existingStyle = seven.GetAttributeValue("style", "");
                        seven.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
            }

            switch (currentfunctioning.AttentionSpanLevel)
            {
                case 1:
                    var one = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='oneI']");
                    if (one != null)
                    {
                        string existingStyle = one.GetAttributeValue("style", "");
                        one.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 2:
                    var two = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='twoI']");
                    if (two != null)
                    {
                        string existingStyle = two.GetAttributeValue("style", "");
                        two.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 3:
                    var three = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='threeI']");
                    if (three != null)
                    {
                        string existingStyle = three.GetAttributeValue("style", "");
                        three.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 4:
                    var four = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='fourI']");
                    if (four != null)
                    {
                        string existingStyle = four.GetAttributeValue("style", "");
                        four.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 5:
                    var five = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='fiveI']");
                    if (five != null)
                    {
                        string existingStyle = five.GetAttributeValue("style", "");
                        five.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 6:
                    var six = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sixI']");
                    if (six != null)
                    {
                        string existingStyle = six.GetAttributeValue("style", "");
                        six.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case 7:
                    var seven = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sevenI']");
                    if (seven != null)
                    {
                        string existingStyle = seven.GetAttributeValue("style", "");
                        seven.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
            }

            // PARENT/CHILD RELATIONSHIP
            htmlDoc.SetInnerHtml("//div[@class='describeparentingyourchild']", parentchildrelationship.ParentingExperience);
            htmlDoc.SetInnerHtml("//div[@class='describemostchallenging']", parentchildrelationship.Challenges);
            htmlDoc.SetInnerHtml("//div[@class='describewhatkindofdiscipline']", parentchildrelationship.DisciplineMethods);

            return htmlDoc.DocumentNode.OuterHtml; // Return updated HTML
        }

        public async Task<string> ModifyHtmlTemplateAsync_Page6(string htmlContent, int id, int assessmentID)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using var context = new ApplicationDbContext(connectionString);

            var patient = await context.Patients.FindAsync(id); 
            var education = await context.Education.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID && p.PatientID == id);
            var employment = await context.Employment.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID && p.PatientID == id);
            var housing = await context.Housing.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID && p.PatientID == id);
            var fostercare = await context.FosterCare.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID && p.PatientID == id);
            var alcoholdrugassessment = await context.AlcoholDrugAssessment.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID && p.PatientID == id);

            if (patient == null)
            {
                return string.Empty;
            }

            // USING HTMLAGILITYPACK
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // EDUCATION
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yescurrentlyenrolledcheckbox", education?.IsCurrentlyEnrolled == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nocurrentlyenrolledcheckbox", education?.IsCurrentlyEnrolled == false);
            
            htmlDoc.SetInnerHtml("//div[@class='Nameofschool']", education.SchoolName);
            htmlDoc.SetInnerHtml("//div[@class='Childgrade']", education.ChildGradeLevel);
            htmlDoc.SetInnerHtml("//div[@class='Childsummergrade']", education.SummerGradeLevel);
            htmlDoc.SetInnerHtml("//div[@class='Describechildattendance']", education.DescribeChildAttendance);

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Attendingregularlycheckbox", education?.ChildAttendance.Safe().ToUpper() == "ATTENDING REGULARLY");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Homeschooledcheckbox", education?.ChildAttendance.Safe().ToUpper() == "HOME-SCHOOLED");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Sometruancycheckbox", education?.ChildAttendance.Safe().ToUpper() == "SOME TRUANCY");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Alternativeschoolcheckbox", education?.ChildAttendance.Safe().ToUpper() == "ALTERNATIVE SCHOOL");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Suspendedcheckbox", education?.ChildAttendance.Safe().ToUpper() == "SUSPENDED");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Expelledcheckbox", education?.ChildAttendance.Safe().ToUpper() == "EXPELLED");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Droppedoutcheckbox", education?.ChildAttendance.Safe().ToUpper() == "DROPPED OUT");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Alternativelearningsystemcheckbox", education?.ChildAttendance.Safe().ToUpper() == "ALTERNATIVE LEARNING SYSTEM (ALS)");
            
            htmlDoc.SetInnerHtml("//div[@class='Describeachievements']", education.DescribeChildAchievements);
            htmlDoc.SetInnerHtml("//div[@class='Describeattitude']", education.DescribeChildAttitude);

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yesdisciplinaryissuescheckbox", education?.HasDisciplinaryIssues == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nodisciplinaryissuescheckbox", education?.HasDisciplinaryIssues == false);
                        
            htmlDoc.SetInnerHtml("//div[@class='Describedisciplinaryissues']", education.DescribeDisciplinaryIssues);

            if (education.HasSpecialEducation)
            {
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Specialeducationcheckbox", education.HasSpecialEducation);
                htmlDoc.SetInnerHtml("//div[@class='Describespecialeducation']", education.DescribeSpecialEducation);
            }
            if (education.HasHomeStudy)
            {
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Homestudycheckbox", education.HasHomeStudy);
                htmlDoc.SetInnerHtml("//div[@class='Describehomestudy']", education.DescribeHomeStudy);
            }
            if (education.HasDiagnosedLearningDisability)
            {
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Diagnosedlearningdisabilitycheckbox", education.HasDiagnosedLearningDisability);
                htmlDoc.SetInnerHtml("//div[@class='Describediagnosedlearningdisability']", education.DescribeDiagnosedLearningDisability);
            }
            if (education.HasSpecialServices)
            {
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Receivingspecialservicescheckbox", education.HasSpecialServices);
                htmlDoc.SetInnerHtml("//div[@class='Describereceivingspecialservices']", education.DescribeSpecialServices);
            }

            // EMPLOYMENT
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yescurrentlyemployedcheckbox", employment?.IsCurrentlyEmployed == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nocurrentlyemployedcheckbox", employment?.IsCurrentlyEmployed == false);
            
            if (employment.IsCurrentlyEmployed)
            {
                htmlDoc.SetInnerHtml("//div[@class='Ifemployedwherearetheyworking']", employment.Location);
                htmlDoc.SetInnerHtml("//div[@class='Howlongaretheyworking']", employment.JobDuration);

                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yesdoesenjoycheckbox", employment?.IsEnjoyingJob == true);
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nodoesenjoycheckbox", employment?.IsEnjoyingJob == false);
                            
            }

            // HOUSING
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Stablecheckbox", housing?.IsStable == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Unstablecheckbox", housing?.IsStable == false);
            
            htmlDoc.SetInnerHtml("//div[@class='Describeunstable']", housing.DescribeIfUnstable);

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Parentguardianownshomecheckbox", housing.HousingType.Safe().ToUpper() == "PARENT/GUARDIAN OWNS HOME");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Parentguardianrentshomecheckbox", housing.HousingType.Safe().ToUpper() == "PARENT/GUARDIAN RENTS HOME");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Temporarycheckbox", housing.HousingType.Safe().ToUpper() == "CHILD AND FAMILY LIVE WITH RELATIVES/FRIENDS (TEMPORARY)");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Permanentcheckbox", housing.HousingType.Safe().ToUpper() == "CHILD AND FAMILY LIVE WITH RELATIVES/FRIENDS (PERMANENT)");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Homelesscheckbox", housing.HousingType.Safe().ToUpper() == "HOMELESS");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Transitionalhousingcheckbox", housing.HousingType.Safe().ToUpper() == "TRANSITIONAL HOUSING");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Emergencysheltercheckbox", housing.HousingType.Safe().ToUpper() == "EMERGENCY SHELTER");
            
            htmlDoc.SetInnerHtml("//div[@class='Howlonghaschildlivedincurrentsituation']", housing.DurationLivedInHouse);
            htmlDoc.SetInnerHtml("//div[@class='Howmanytimeshaschildmoved']", housing.TimesMoved);
            htmlDoc.SetInnerHtml("//div[@class='Housingadditionalinfo']", housing.AdditionalInfo);

            // FOSTER CARE
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yeshasbeeninfostercarecheckbox", fostercare?.HasBeenFosterCared.Safe().ToUpper() == "YES");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nohasbeeninfostercarecheckbox", fostercare?.HasBeenFosterCared.Safe().ToUpper() == "NO");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Unknownhasbeeninfostercarecheckbox", fostercare?.HasBeenFosterCared.Safe().ToUpper() == "UNKNOWN");
            
            if (fostercare.HasBeenFosterCared == "Yes")
            {
                htmlDoc.SetInnerHtml("//div[@class='Fromage']", fostercare.FosterAgeStart);
                htmlDoc.SetInnerHtml("//div[@class='Toage']", fostercare.FosterAgeEnd);
                htmlDoc.SetInnerHtml("//div[@class='Describereasonfostercare']", fostercare.Reason);

                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Familialplacementcheckbox", fostercare?.PlacementType.Safe().ToUpper() == "FAMILIAL PLACEMENT");
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nonfamilialplacementcheckbox", fostercare?.PlacementType.Safe().ToUpper() == "NON-FAMILIAL PLACEMENT");
                
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Incarecheckbox", fostercare?.CurrentStatus.Safe().ToUpper() == "IN-CARE");
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Outofcarecheckbox", fostercare?.CurrentStatus.Safe().ToUpper() == "OUT OF CARE");
                
                switch (fostercare?.OutOfCareReason.Safe().ToUpper())
                {
                    case "ADOPTED":
                        var adopted = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Adoptedcheckbox']");
                        if (adopted != null)
                        {
                            string existingStyle = adopted.GetAttributeValue("style", "");
                            adopted.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                    case "RETURNED TO HOME":
                        var returnedtohome = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Returnedtohomecheckbox']");
                        if (returnedtohome != null)
                        {
                            string existingStyle = returnedtohome.GetAttributeValue("style", "");
                            returnedtohome.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                    case "EMANCIPATED":
                        var emancipated = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Emancipatedcheckbox']");
                        if (emancipated != null)
                        {
                            string existingStyle = emancipated.GetAttributeValue("style", "");
                            emancipated.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                    case "RAN AWAY FROM CARE":
                        var ranaway = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Ranawayfromcarecheckbox']");
                        if (ranaway != null)
                        {
                            string existingStyle = ranaway.GetAttributeValue("style", "");
                            ranaway.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                    default:
                        var other = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Othersoutofcarecheckbox']");
                        if (other != null)
                        {
                            string existingStyle = other.GetAttributeValue("style", "");
                            other.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        var otherreason = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Describeotherreason']");
                        if (otherreason != null)
                        {
                            otherreason.InnerHtml = fostercare.OutOfCareReason;
                        }
                        break;
                }
            }

            // ALCOHOL/DRUG ASSESSMENT
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yessmoketobaccocheckbox", alcoholdrugassessment?.TobaccoUse.Safe().ToUpper() == "YES");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nosmoketobaccocheckbox", alcoholdrugassessment?.TobaccoUse.Safe().ToUpper() == "NO");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Donotknowsmoketobaccocheckbox", alcoholdrugassessment?.TobaccoUse.Safe().ToUpper() == "DO NOT KNOW");
            
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yesusealcoholcheckbox", alcoholdrugassessment?.AlcoholUse.Safe().ToUpper() == "YES");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nousealcoholcheckbox", alcoholdrugassessment?.AlcoholUse.Safe().ToUpper() == "NO");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Donotknowusealcoholcheckbox", alcoholdrugassessment?.AlcoholUse.Safe().ToUpper() == "DO NOT KNOW");

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yesrecreationallycheckbox", alcoholdrugassessment?.RecreationalMedicationUse.Safe().ToUpper() == "YES");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Norecreationallycheckbox", alcoholdrugassessment?.RecreationalMedicationUse.Safe().ToUpper() == "NO");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Donotknowrecreationallycheckbox", alcoholdrugassessment?.RecreationalMedicationUse.Safe().ToUpper() == "DO NOT KNOW");
            
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yesoverdosedcheckbox", alcoholdrugassessment?.HasOverdosed == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nooverdosedcheckbox", alcoholdrugassessment?.HasOverdosed == false);
            
            htmlDoc.SetInnerHtml("//div[@class='Describewhenoverdose']", alcoholdrugassessment.OverdoseDate);

            return htmlDoc.DocumentNode.OuterHtml; // Return updated HTML

        }

        public async Task<string> ModifyHtmlTemplateAsync_Page7(string htmlContent, int id, int assessmentID)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using var context = new ApplicationDbContext(connectionString);

            var patient = await context.Patients.FindAsync(id);
            var alcoholdrugassessment = await context.AlcoholDrugAssessment.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID && p.PatientID == id);
            var legalinvolvement = await context.LegalInvolvement.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID && p.PatientID == id);
            var historyofabuse = await context.HistoryOfAbuse.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID && p.PatientID == id);
            var historyofviolence = await context.HistoryOfViolence.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID && p.PatientID == id);

            if (patient == null)
            {
                return string.Empty;
            }

            // USING HTMLAGILITYPACK
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // ALCOHOL/DRUG ASSESSMENT
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yesexperiencedproblemscheckbox", alcoholdrugassessment?.HasAlcoholProblems == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Noexperiencedproblemscheckbox", alcoholdrugassessment?.HasAlcoholProblems == false);
            
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Legalcheckbox", alcoholdrugassessment.LegalProblems);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Socialpeercheckbox", alcoholdrugassessment.SocialPeerProblems);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Workcheckbox", alcoholdrugassessment.WorkProblems);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Familycheckbox", alcoholdrugassessment.FamilyProblems);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Friendscheckbox", alcoholdrugassessment.FriendsProblems);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Financialcheckbox", alcoholdrugassessment.FinancialProblems);
            
            htmlDoc.SetInnerHtml("//div[@class='Describealcoholproblems']", alcoholdrugassessment.DescribeProblems);

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yeshavecontinuedcheckbox", alcoholdrugassessment?.ContinuedUse == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nohavecontinuedcheckbox", alcoholdrugassessment?.ContinuedUse == false);
            
            // LEGAL INVOLVEMENT
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yescurrentcustodycheckbox", legalinvolvement?.HasCustodyCase == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nocurrentcustodycheckbox", legalinvolvement?.HasCustodyCase == false);
           
            htmlDoc.SetInnerHtml("//div[@class='Describecurrentcustody']", legalinvolvement.DescribeCustodyCase);

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nonecheckbox", legalinvolvement?.HasCPSInvolvement.Safe().ToUpper() == "NONE");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Pastcheckbox", legalinvolvement?.HasCPSInvolvement.Safe().ToUpper() == "PAST");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Currentcheckbox", legalinvolvement?.HasCPSInvolvement.Safe().ToUpper() == "CURRENT");
            
            htmlDoc.SetInnerHtml("//div[@class='Describechildprotectiveservice']", legalinvolvement.DescribeCPSInvolvement);

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Noinvolvementcheckbox", legalinvolvement?.LegalStatus.Safe().ToUpper() == "NO INVOLVEMENT");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Probationlengthcheckbox", legalinvolvement?.LegalStatus.Safe().ToUpper() == "PROBATION");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Parolelengthcheckbox", legalinvolvement?.LegalStatus.Safe().ToUpper() == "PAROLE");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Chargespendingcheckbox", legalinvolvement?.LegalStatus.Safe().ToUpper() == "PENDING CHARGES");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Priorincarcerationcheckbox", legalinvolvement?.LegalStatus.Safe().ToUpper() == "PRIOR INCARCERATION");
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Lawsuitcheckbox", legalinvolvement?.LegalStatus.Safe().ToUpper() == "LAW SUIT OR OTHER COURT PROCEEDING");
            
            htmlDoc.SetInnerHtml("//div[@class='Describecharges']", legalinvolvement.Charges);
            htmlDoc.SetInnerHtml("//div[@class='Officername']", legalinvolvement.OfficerName);
            htmlDoc.SetInnerHtml("//div[@class='Contactnum']", legalinvolvement.OfficerContactNum.Safe() == "0" ? "" : legalinvolvement.OfficerContactNum.Safe());
            htmlDoc.SetInnerHtml("//div[@class='Legaladditionalinfo']", legalinvolvement.AdditionalInfo);

            // HISTORY OF ABUSE
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yeshasbeenabusedcheckbox", historyofabuse?.HasBeenAbused == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nohasbeenabusedcheckbox", historyofabuse?.HasBeenAbused == false);

            if (historyofabuse.SexualAbuse)
            {
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Sexualabusecheckbox", historyofabuse.SexualAbuse);
                
                htmlDoc.SetInnerHtml("//div[@class='Bywhom1']", historyofabuse.SexualAbuseByWhom);
                htmlDoc.SetInnerHtml("//div[@class='Atwhatage1']", historyofabuse.SexualAbuseAgeOfChild);

                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yeswasitreported1checkbox", historyofabuse?.SexualAbuseReported == true);
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nowasitreported1checkbox", historyofabuse?.SexualAbuseReported == false);
                
            }

            if (historyofabuse.PhysicalAbuse)
            {
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Physicalabusecheckbox", historyofabuse.PhysicalAbuse);
                
                htmlDoc.SetInnerHtml("//div[@class='Bywhom2']", historyofabuse.PhysicalAbuseByWhom);
                htmlDoc.SetInnerHtml("//div[@class='Atwhatage2']", historyofabuse.PhysicalAbuseAgeOfChild);

                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yeswasitreported2checkbox", historyofabuse?.PhysicalAbuseReported == true);
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nowasitreported2checkbox", historyofabuse?.PhysicalAbuseReported == false);
                
            }

            if (historyofabuse.EmotionalAbuse)
            {
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Emotionalabusecheckbox", historyofabuse.EmotionalAbuse);

                htmlDoc.SetInnerHtml("//div[@class='Bywhom3']", historyofabuse.EmotionalAbuseByWhom);
                htmlDoc.SetInnerHtml("//div[@class='Atwhatage3']", historyofabuse.EmotionalAbuseAgeOfChild);

                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yeswasitreported3checkbox", historyofabuse?.EmotionalAbuseReported == true);
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nowasitreported3checkbox", historyofabuse?.EmotionalAbuseReported == false);
                
            }

            if (historyofabuse.VerbalAbuse)
            {
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Verbalabusecheckbox", historyofabuse.VerbalAbuse);

                htmlDoc.SetInnerHtml("//div[@class='Bywhom4']", historyofabuse.VerbalAbuseByWhom);
                htmlDoc.SetInnerHtml("//div[@class='Atwhatage4']", historyofabuse.VerbalAbuseAgeOfChild);

                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yeswasitreported4checkbox", historyofabuse?.VerbalAbuseReported == true);
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nowasitreported4checkbox", historyofabuse?.VerbalAbuseReported == false);
                
            }

            if (historyofabuse.AbandonedAbuse)
            {
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Abandonedneglectedabusecheckbox", historyofabuse.AbandonedAbuse);

                htmlDoc.SetInnerHtml("//div[@class='Bywhom5']", historyofabuse.AbandonedAbuseByWhom);
                htmlDoc.SetInnerHtml("//div[@class='Atwhatage5']", historyofabuse.AbandonedAbuseAgeOfChild);
                
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yeswasitreported5checkbox", historyofabuse?.AbandonedAbuseReported == true);
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nowasitreported5checkbox", historyofabuse?.AbandonedAbuseReported == false);
                
            }

            if (historyofabuse.PsychologicalAbuse)
            {
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Psychologicalabusecheckbox", historyofabuse.PsychologicalAbuse);
                
                htmlDoc.SetInnerHtml("//div[@class='Bywhom6']", historyofabuse.PsychologicalAbuseByWhom);
                htmlDoc.SetInnerHtml("//div[@class='Atwhatage6']", historyofabuse.PsychologicalAbuseAgeOfChild);

                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yeswasitreported6checkbox", historyofabuse?.PsychologicalAbuseReported == true);
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nowasitreported6checkbox", historyofabuse?.PsychologicalAbuseReported == false);
                
            }

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yesvictimofbullyingcheckbox", historyofabuse?.VictimOfBullying == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Novictimofbullyingcheckbox", historyofabuse?.VictimOfBullying == false);
            
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yessafetyconcernscheckbox", historyofabuse?.SafetyConcerns == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nosafetyconcernscheckbox", historyofabuse?.SafetyConcerns == false);

            htmlDoc.SetInnerHtml("//div[@class='Describesafetyconcerns']", historyofabuse.DescribeSafetyConcerns);
            htmlDoc.SetInnerHtml("//div[@class='Historyofabuseadditionalinfo']", historyofabuse.AdditionalInfo);

            // HISTORY OF VIOLENCE
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yeshasbeenaccusedcheckbox", historyofviolence?.HasBeenAccused == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Nohasbeenaccusedcheckbox", historyofviolence?.HasBeenAccused == false);
           
            if (historyofviolence.SexualAbuse)
            {
                htmlDoc.SetInnerHtml("//div[@class='Accusedtowhom1']", historyofviolence.SexualAbuseToWhom);
                htmlDoc.SetInnerHtml("//div[@class='Accusedatwhatage1']", historyofviolence.SexualAbuseAgeOfChild);

                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Accusedyeswasitreported1checkbox", historyofviolence?.SexualAbuseReported == true);
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Accusednowasitreported1checkbox", historyofviolence?.SexualAbuseReported == false);
                
            }

            if (historyofviolence.PhysicalAbuse)
            {
                htmlDoc.SetInnerHtml("//div[@class='Accusedtowhom2']", historyofviolence.PhysicalAbuseToWhom);
                htmlDoc.SetInnerHtml("//div[@class='Accusedatwhatage2']", historyofviolence.PhysicalAbuseAgeOfChild);

                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Accusedyeswasitreported2checkbox", historyofviolence?.PhysicalAbuseReported == true);
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Accusednowasitreported2checkbox", historyofviolence?.PhysicalAbuseReported == false);
                
            }

            if (historyofviolence.EmotionalAbuse)
            {
                htmlDoc.SetInnerHtml("//div[@class='Accusedtowhom3']", historyofviolence.EmotionalAbuseToWhom);
                htmlDoc.SetInnerHtml("//div[@class='Accusedatwhatage3']", historyofviolence.EmotionalAbuseAgeOfChild);

                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Accusedyeswasitreported3checkbox", historyofviolence?.EmotionalAbuseReported == true);
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Accusednowasitreported3checkbox", historyofviolence?.EmotionalAbuseReported == false);
                
            }

            if (historyofviolence.VerbalAbuse)
            {
                htmlDoc.SetInnerHtml("//div[@class='Accusedtowhom4']", historyofviolence.VerbalAbuseToWhom);
                htmlDoc.SetInnerHtml("//div[@class='Accusedatwhatage4']", historyofviolence.VerbalAbuseAgeOfChild);

                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Accusedyeswasitreported4checkbox", historyofviolence?.VerbalAbuseReported == true);
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Accusednowasitreported4checkbox", historyofviolence?.VerbalAbuseReported == false);
                
            }

            if (historyofviolence.AbandonedAbuse)
            {
                htmlDoc.SetInnerHtml("//div[@class='Accusedtowhom5']", historyofviolence.AbandonedAbuseToWhom);
                htmlDoc.SetInnerHtml("//div[@class='Accusedatwhatage5']", historyofviolence.AbandonedAbuseAgeOfChild);

                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Accusedyeswasitreported5checkbox", historyofviolence?.AbandonedAbuseReported == true);
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Accusednowasitreported5checkbox", historyofviolence?.AbandonedAbuseReported == false);
                
            }

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Yesknowntobullycheckbox", historyofviolence?.Bullying == true);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Noknowntobullycheckbox", historyofviolence?.Bullying == false);
            
            htmlDoc.SetInnerHtml("//div[@class='Historyofviolenceadditionalinfo']", historyofviolence.AdditionalInfo);

            return htmlDoc.DocumentNode.OuterHtml; // Return updated HTML
        }

        public async Task<string> ModifyHtmlTemplateAsync_Page8(string htmlContent, int id, int assessmentID)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using var context = new ApplicationDbContext(connectionString);

            var patient = await context.Patients.FindAsync(id);
            var strengthsresources = await context.StrengthsResources.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID && p.PatientID == id);
            var goals = await context.Goals.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID && p.PatientID == id);
            var assessments = await context.Assessments.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID && p.PatientID == id);

            if (patient == null)
            {
                return string.Empty;
            }

            // USING HTMLAGILITYPACK
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // STRENGTHS AND RESOURCES
            htmlDoc.SetInnerHtml("//div[@class='Strengths']", strengthsresources.Strengths);
            htmlDoc.SetInnerHtml("//div[@class='Limitations']", strengthsresources.Limitations);
            htmlDoc.SetInnerHtml("//div[@class='Resources']", strengthsresources.Resources);
            htmlDoc.SetInnerHtml("//div[@class='Experiences']", strengthsresources.Experiences);
            htmlDoc.SetInnerHtml("//div[@class='Alreadydoing']", strengthsresources.AlreadyDoing);

            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Parentscheckbox", strengthsresources.ParentsSupport);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Boyfriendgirlfriendcheckbox", strengthsresources.PartnerSupport);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Siblingscheckbox", strengthsresources.SiblingsSupport);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Extendedfamilycheckbox", strengthsresources.ExtendedFamilySupport);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Friendscheckbox", strengthsresources.FriendsSupport);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Neighborscheckbox", strengthsresources.NeighborsSupport);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Schoolstaffcheckbox", strengthsresources.SchoolStaffSupport);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Churchcheckbox", strengthsresources.ChurchSupport);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Pastorcheckbox", strengthsresources.PastorSupport);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Therapistcheckbox", strengthsresources.TherapistSupport);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Groupcheckbox", strengthsresources.GroupSupport);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Communityservicecheckbox", strengthsresources.CommunityServiceSupport);
            HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Doctorcheckbox", strengthsresources.DoctorSupport);
            
            if (strengthsresources.OthersSupport)
            {
                HtmlTemplateHelper.SetCheckboxStyle(htmlDoc, "Othercheckbox", strengthsresources.OthersSupport);
                htmlDoc.SetInnerHtml("//div[@class='Otherssix']", strengthsresources.Others);
            }

            // GOALS
            htmlDoc.SetInnerHtml("//div[@class='Biggestneedrightnow']", goals.CurrentNeeds);
            htmlDoc.SetInnerHtml("//div[@class='Hopetogain']", goals.HopeToGain);
            htmlDoc.SetInnerHtml("//div[@class='Goal1text']", goals.Goal1);
            htmlDoc.SetInnerHtml("//div[@class='Goal2text']", goals.Goal2);
            htmlDoc.SetInnerHtml("//div[@class='Goal3text']", goals.Goal3);
            htmlDoc.SetInnerHtml("//div[@class='Goalsadditionalinfo']", goals.AdditionalInfo);

            // ASSESSMENTS
            htmlDoc.SetInnerHtml("//div[@class='Assessmentstatement']", assessments.AssessmentStatement);

            return htmlDoc.DocumentNode.OuterHtml; // Return updated HTML
        }

        public async Task<List<string>> ModifyHtmlTemplateAsync_Page9(string htmlContent, int id, int assessmentID)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using var context = new ApplicationDbContext(connectionString);

            var patient = await context.Patients.FindAsync(id);
            var assessments = await context.Assessments.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID && p.PatientID == id);

            int notesPerPage = 29;
            var notes = await context.ProgressNotes
                .Where(p => p.PatientID == id && p.AssessmentID == assessmentID)
                .ToListAsync(); 

            var pagedNotes = notes
                .Select((note, index) => new { note, index })
                .GroupBy(x => x.index / notesPerPage)
                .Select(g => g.Select(x => x.note).ToList())
                .ToList(); 

            var fullPagesHtml = new List<string>();

            foreach (var notesPage in pagedNotes)
            {
                // UPDATE LOGO IMAGE
                string imagePath = Path.Combine(_environment.WebRootPath, "resources", "NCH-Logo.png");
                byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(imagePath);
                string base64String = Convert.ToBase64String(imageBytes);
                htmlContent = htmlContent.Replace("/resources/NCH-Logo.png", $"data:image/png;base64,{base64String}");

                // USING HTMLAGILITYPACK
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);

                // Find the placeholder
                var container = htmlDoc.GetElementbyId("progress-notes-container");

                // Generate dynamic content
                var topOffset = 115;
                var spacing = 23;
                var sb = new StringBuilder();

                foreach (var note in notesPage)
                {
                    sb.Append($@"
                    <div>
                        <div style='width: 492px; height: 24px; left: 84px; top: {topOffset}px; position: absolute; overflow: hidden; outline: 1px black solid; outline-offset: -1px'>
                            <div style='width: 480px; height: 8px; left: 6px; top: 8px; position: absolute; text-align: center; justify-content: center; display: flex; flex-direction: column; color: black; font-size: 8px; font-family: Inter; font-weight: 400; word-wrap: break-word'>
                            {note.ProgressNotes.Safe()}
                            </div>
                        </div>
                        <div style='width: 67px; height: 24px; left: 18px; top: {topOffset}px; position: absolute; overflow: hidden; outline: 1px black solid; outline-offset: -1px'>
                            <div style='width: 59px; height: 6px; left: 4px; top: 9px; position: absolute; text-align: center; justify-content: center; display: flex; flex-direction: column; color: black; font-size: 8px; font-family: Inter; font-weight: 400; word-wrap: break-word'>
                            {note.Date.Safe()}
                            </div>
                        </div>
                    </div>");
                    topOffset += spacing;
                }

                container.InnerHtml = sb.ToString();
                fullPagesHtml.Add(htmlDoc.DocumentNode.OuterHtml);
            }

            return fullPagesHtml; // Return updated HTML
        }
    }
}
