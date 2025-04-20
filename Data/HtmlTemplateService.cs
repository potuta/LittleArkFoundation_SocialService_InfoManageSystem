using HtmlAgilityPack;
using LittleArkFoundation.Areas.Admin.Models.Housing;
using Microsoft.AspNetCore.Html;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Net.Http;

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

        public async Task<string> ModifyHtmlTemplateAsync_Page1(string htmlContent, int id)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using var context = new ApplicationDbContext(connectionString);

            var patient = await context.Patients.FindAsync(id);
            var assessment = await context.Assessments.FirstOrDefaultAsync(a => a.PatientID == id);
            var referral = await context.Referrals.FirstOrDefaultAsync(r => r.PatientID == id);
            var informant = await context.Informants.FirstOrDefaultAsync(i => i.PatientID == id);
            var familymembers = await context.FamilyComposition
                                .Where(f => f.PatientID == id)
                                .ToListAsync();
            var household = await context.Households.FirstOrDefaultAsync(h => h.PatientID == id);
            var mswdclassification = await context.MSWDClassification.FirstOrDefaultAsync(m => m.PatientID == id);
            
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
            var dateofinterview = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Dateofinterview']");
            if (dateofinterview != null)
            {
                dateofinterview.InnerHtml = assessment.DateOfInterview.ToString();
            }

            var timeofinterview = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Timeofinterview']");
            if (timeofinterview != null)
            {
                timeofinterview.InnerHtml = assessment.TimeOfInterview.ToString();
            }

            var basicward = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Basicward']");
            if (basicward != null)
            {
                basicward.InnerHtml = assessment.BasicWard;
            }

            var nonbasicward = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nonbasicward']");
            if (nonbasicward != null)
            {
                nonbasicward.InnerHtml = assessment.NonBasicWard;
            }

            var healthrecordno = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Healthrecord']");
            if (healthrecordno != null)
            {
                healthrecordno.InnerHtml = assessment.HealthRecordNo;
            }

            var mswdno = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Mswdno']");
            if (mswdno != null)
            {
                mswdno.InnerHtml = assessment.MSWDNo;
            }

            // REFERRALS
            var referralname = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Namereferral']");
            if (referralname != null)
            {
                referralname.InnerHtml = referral.Name;
            }

            var referraladdress = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Addressreferral']");
            if (referraladdress != null)
            {
                referraladdress.InnerHtml = referral.Address;
            }

            var referralcontactno = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Contactnoreferral']");
            if (referralcontactno != null)
            {
                referralcontactno.InnerHtml = referral.ContactNo;
            }

            // INFORMANTS
            var informantname = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Informantname']");
            if (informantname != null)
            {
                informantname.InnerHtml = informant.Name;
            }

            var relationtopatient = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Relationtopatient']");
            if (relationtopatient != null)
            {
                relationtopatient.InnerHtml = informant.RelationToPatient;
            }

            var informantcontactno = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Contactnoinformant']");
            if (informantcontactno != null)
            {
                informantcontactno.InnerHtml = informant.ContactNo;
            }

            var informantaddress = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Addressinformant']");
            if (informantaddress != null)
            {
                informantaddress.InnerHtml = informant.Address;
            }

            // PATIENTS
            var patientlastname = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Patientsurname']");
            if (patientlastname != null)
            {
                patientlastname.InnerHtml = patient.LastName;
            }

            var patientfirstname = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Patientfirstname']");
            if (patientfirstname != null)
            {
                patientfirstname.InnerHtml = patient.FirstName;
            }

            var patientmiddlename = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Patientmiddlename']");
            if (patientmiddlename != null)
            {
                patientmiddlename.InnerHtml = patient.MiddleName;
            }

            var patientdateofbirth = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Dateofbirth']");
            if (patientdateofbirth != null)
            {
                patientdateofbirth.InnerHtml = patient.DateOfBirth.ToString();
            }

            var patientage = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Age']");
            if (patientage != null)
            {
                patientage.InnerHtml = patient.Age.ToString();
            }

            switch (patient.Sex)
            {
                case "Male":
                    var sexmalecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Sexmalecheckbox']");
                    if (sexmalecheckbox != null)
                    {
                        string existingStyle = sexmalecheckbox.GetAttributeValue("style", "");
                        sexmalecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Female":
                    var sexfemalecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Sexfemalecheckbox']");
                    if (sexfemalecheckbox != null)
                    {
                        string existingStyle = sexfemalecheckbox.GetAttributeValue("style", "");
                        sexfemalecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var patientcontactno = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Contactnopatient']");
            if (patientcontactno != null)
            {
                patientcontactno.InnerHtml = patient.ContactNo;
            }

            var patientplaceofbirth = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Placeofbirth']");
            if (patientplaceofbirth != null)
            {
                patientplaceofbirth.InnerHtml = patient.PlaceOfBirth;
            }

            switch (patient.Gender)
            {
                case "Male":
                    var gendermalecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Gendermalecheckbox']");
                    if (gendermalecheckbox != null)
                    {
                        string existingStyle = gendermalecheckbox.GetAttributeValue("style", "");
                        gendermalecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Female":
                    var genderfemalecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Genderfemalecheckbox']");
                    if (genderfemalecheckbox != null)
                    {
                        string existingStyle = genderfemalecheckbox.GetAttributeValue("style", "");
                        genderfemalecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "LGBTQIA+":
                    var genderlgbtqcheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Genderlgbtqcheckbox']");
                    if (genderlgbtqcheckbox != null)
                    {
                        string existingStyle = genderlgbtqcheckbox.GetAttributeValue("style", "");
                        genderlgbtqcheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var patientreligion = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Religion']");
            if (patientreligion != null)
            {
                patientreligion.InnerHtml = patient.Religion;
            }

            var patientnationality = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nationality']");
            if (patientnationality != null)
            {
                patientnationality.InnerHtml = patient.Nationality;
            }

            var permanentaddress = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Permanentaddress']");
            if (permanentaddress != null)
            {
                permanentaddress.InnerHtml = patient.PermanentAddress;
            }

            var temporaryaddress = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Temporaryaddress']");
            if (temporaryaddress != null)
            {
                temporaryaddress.InnerHtml = patient.TemporaryAddress;
            }

            switch (patient.CivilStatus)
            {
                case "Legitimate":
                    var civillegitimatecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Legitimatecheckbox']");
                    if (civillegitimatecheckbox != null)
                    {
                        string existingStyle = civillegitimatecheckbox.GetAttributeValue("style", "");
                        civillegitimatecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Illegitimate":
                    var civilillegitimatecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Illegitimatecheckbox']");
                    if (civilillegitimatecheckbox != null)
                    {
                        string existingStyle = civilillegitimatecheckbox.GetAttributeValue("style", "");
                        civilillegitimatecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Married":
                    var civilmarriedcheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Marriedcheckbox']");
                    if (civilmarriedcheckbox != null)
                    {
                        string existingStyle = civilmarriedcheckbox.GetAttributeValue("style", "");
                        civilmarriedcheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Common Law":
                    var civilcommonlawcheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Commonlawcheckbox']");
                    if (civilcommonlawcheckbox != null)
                    {
                        string existingStyle = civilcommonlawcheckbox.GetAttributeValue("style", "");
                        civilcommonlawcheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Widowed":
                    var civilwidowedcheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Widowedcheckbox']");
                    if (civilwidowedcheckbox != null)
                    {
                        string existingStyle = civilwidowedcheckbox.GetAttributeValue("style", "");
                        civilwidowedcheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            switch (patient.EducationLevel)
            {
                case "Primary":
                    var primarycheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Primarycheckbox']");
                    if (primarycheckbox != null)
                    {
                        string existingStyle = primarycheckbox.GetAttributeValue("style", "");
                        primarycheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Secondary":
                    var secondarycheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Secondarycheckbox']");
                    if (secondarycheckbox != null)
                    {
                        string existingStyle = secondarycheckbox.GetAttributeValue("style", "");
                        secondarycheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Vocational":
                    var vocationalcheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Vocationalcheckbox']");
                    if (vocationalcheckbox != null)
                    {
                        string existingStyle = vocationalcheckbox.GetAttributeValue("style", "");
                        vocationalcheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Tertiary":
                    var tertiarycheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Tertiarycheckbox']");
                    if (tertiarycheckbox != null)
                    {
                        string existingStyle = tertiarycheckbox.GetAttributeValue("style", "");
                        tertiarycheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "No Educational Attainment":
                    var noeducationcheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Noeducationcheckbox']");
                    if (noeducationcheckbox != null)
                    {
                        string existingStyle = noeducationcheckbox.GetAttributeValue("style", "");
                        noeducationcheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var patientoccupation = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Occupation']");
            if (patientoccupation != null)
            {
                patientoccupation.InnerHtml = patient.Occupation;
            }

            var patientmonthlyincome = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Patientsmonthlyincome']");
            if (patientmonthlyincome != null)
            {
                patientmonthlyincome.InnerHtml = patient.MonthlyIncome.ToString();
            }

            var philhealthpin = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Pin']");
            if (philhealthpin != null)
            {
                philhealthpin.InnerHtml = patient.PhilhealthPIN.ToString();
            }

            switch (patient.PhilhealthMembership)
            {
                case "Direct Contributor":
                    var directcheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Directcontributorcheckbox']");
                    if (directcheckbox != null)
                    {
                        string existingStyle = directcheckbox.GetAttributeValue("style", "");
                        directcheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Indirect Contributor":
                    var indirectcheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Indirectcontributorcheckbox']");
                    if (indirectcheckbox != null)
                    {
                        string existingStyle = indirectcheckbox.GetAttributeValue("style", "");
                        indirectcheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            // FAMILY COMPOSITION
            int i = 1;
            foreach (var familyMember in familymembers)
            {
                var name = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Familyname{i}']");
                if (name != null)
                {
                    name.InnerHtml = familyMember.Name;
                }

                var age = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Familyage{i}']");
                if (age != null)
                {
                    age.InnerHtml = familyMember.Age.ToString();
                }

                var dateofbirth = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Familydateofbirth{i}']");
                if (dateofbirth != null)
                {
                    dateofbirth.InnerHtml = familyMember.DateOfBirth.ToString();
                }

                var civilstatus = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Familycivilstatus{i}']");
                if (civilstatus != null)
                {
                    civilstatus.InnerHtml = familyMember.CivilStatus;
                }

                var relationship = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Familyrelationshiptopatient{i}']");
                if (relationship != null)
                {
                    relationship.InnerHtml = familyMember.RelationshipToPatient;
                }

                switch (familyMember.LivingWithChild)
                {
                    case true:
                        var livingwithchildyes = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Yesfamilylivingwithchild{i}']");
                        if (livingwithchildyes != null)
                        {
                            string existingStyle = livingwithchildyes.GetAttributeValue("style", "");
                            livingwithchildyes.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                    case false:
                        var livingwithchildno = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Nofamilylivingwithchild{i}']");
                        if (livingwithchildno != null)
                        {
                            string existingStyle = livingwithchildno.GetAttributeValue("style", "");
                            livingwithchildno.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                }

                var educationlevel = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Familyeducationalattainment{i}']");
                if (educationlevel != null)
                {
                    educationlevel.InnerHtml = familyMember.EducationalAttainment;
                }

                var occupation = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Familyoccupation{i}']");
                if (occupation != null)
                {
                    occupation.InnerHtml = familyMember.Occupation;
                }

                var monthlyincome = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Familymonthlyincome{i}']");
                if (monthlyincome != null)
                {
                    monthlyincome.InnerHtml = familyMember.MonthlyIncome.ToString();
                }

                i++;
            }

            var othersourcesofincome = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Othersourcesofincome']");
            if (othersourcesofincome != null)
            {
                othersourcesofincome.InnerHtml = household.OtherSourcesOfIncome;
            }

            var householdsize = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Householdsize']");
            if (householdsize != null)
            {
                householdsize.InnerHtml = household.HouseholdSize.ToString();
            }

            var totalhouseholdincome = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Totalhouseholdincome']");
            if (totalhouseholdincome != null)
            {
                totalhouseholdincome.InnerHtml = household.TotalHouseholdIncome.ToString();
            }

            var percapitaincome = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Percapitaincome']");
            if (percapitaincome != null)
            {
                percapitaincome.InnerHtml = household.PerCapitaIncome.ToString();
            }

            // MSWD CLASSIFICATION
            switch (mswdclassification.MainClassification)
            {
                case "Financially Capable/Capacitated":
                    var financiallycapable = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Financiallycapablecheckbox']");
                    if (financiallycapable != null)
                    {
                        string existingStyle = financiallycapable.GetAttributeValue("style", "");
                        financiallycapable.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Financially Incapable/Incapacitated":
                    var financiallyincapable = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Financiallyincapablecheckbox']");
                    if (financiallyincapable != null)
                    {
                        string existingStyle = financiallyincapable.GetAttributeValue("style", "");
                        financiallyincapable.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Indigent":
                    var indigent = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Indigentcheckbox']");
                    if (indigent != null)
                    {
                        string existingStyle = indigent.GetAttributeValue("style", "");
                        indigent.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            switch (mswdclassification.SubClassification)
            {
                case "C1":
                    var c1 = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='C1checkbox']");
                    if (c1 != null)
                    {
                        string existingStyle = c1.GetAttributeValue("style", "");
                        c1.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "C2":
                    var c2 = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='C2checkbox']");
                    if (c2 != null)
                    {
                        string existingStyle = c2.GetAttributeValue("style", "");
                        c2.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "C3":
                    var c3 = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='C3checkbox']");
                    if (c3 != null)
                    {
                        string existingStyle = c3.GetAttributeValue("style", "");
                        c3.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            switch (mswdclassification.MembershipSector)
            {
                case "Artisenal Fisher Folk":
                    var artisenalfisherfolk = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Artisenalcheckbox']");
                    if (artisenalfisherfolk != null)
                    {
                        string existingStyle = artisenalfisherfolk.GetAttributeValue("style", "");
                        artisenalfisherfolk.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Farmer and Landless Rural Worker":
                    var farmerandlandless = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Farmercheckbox']");
                    if (farmerandlandless != null)
                    {
                        string existingStyle = farmerandlandless.GetAttributeValue("style", "");
                        farmerandlandless.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Urban Poor":
                    var urbanpoor = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Urbancheckbox']");
                    if (urbanpoor != null)
                    {
                        string existingStyle = urbanpoor.GetAttributeValue("style", "");
                        urbanpoor.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Indigenous Peoples":
                    var indigenouspeople = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Indigenouscheckbox']");
                    if (indigenouspeople != null)
                    {
                        string existingStyle = indigenouspeople.GetAttributeValue("style", "");
                        indigenouspeople.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Senior Citizen":
                    var seniorcitizen = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Seniorcitizencheckbox']");
                    if (seniorcitizen != null)
                    {
                        string existingStyle = seniorcitizen.GetAttributeValue("style", "");
                        seniorcitizen.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Formal Labor and Migrant Workers":
                    var formallabor = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Formallaborcheckbox']");
                    if (formallabor != null)
                    {
                        string existingStyle = formallabor.GetAttributeValue("style", "");
                        formallabor.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Workers in Informal Sector":
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
                case "Victims Of Disaster and Calamity":
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

        public async Task<string> ModifyHtmlTemplateAsync_Page2(string htmlContent, int id)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using var context = new ApplicationDbContext(connectionString);

            var patient = await context.Patients.FindAsync(id);
            var monthlyexpenses = await context.MonthlyExpenses.FirstOrDefaultAsync(m => m.PatientID == id);
            var utilities = await context.Utilities.FirstOrDefaultAsync(u => u.PatientID == id);
            var medicalhistory = await context.MedicalHistory.FirstOrDefaultAsync(m => m.PatientID == id);
            var childhealth = await context.ChildHealth.FirstOrDefaultAsync(c => c.PatientID == id);

            if (patient == null)
            {
                return string.Empty;
            }

            // USING HTMLAGILITYPACK
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // MONTHLY EXPENSES
            var houseandlot = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Houseandlot']");
            if (houseandlot != null)
            {
                houseandlot.InnerHtml = monthlyexpenses.HouseAndLot.ToString();
            }

            var foodandwater = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Foodandwater']");
            if (foodandwater != null)
            {
                foodandwater.InnerHtml = monthlyexpenses.FoodAndWater.ToString();
            }

            var education = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Education']");
            if (education != null)
            {
                education.InnerHtml = monthlyexpenses.Education.ToString();
            }

            var clothing = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Clothing']");
            if (clothing != null)
            {
                clothing.InnerHtml = monthlyexpenses.Clothing.ToString();
            }

            var communication = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Communication']");
            if (communication != null)
            {
                communication.InnerHtml = monthlyexpenses.Communication.ToString();
            }

            var househelp = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Househelp']");
            if (househelp != null)
            {
                househelp.InnerHtml = monthlyexpenses.HouseHelp.ToString();
            }

            var medicalexpenses = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Medicalexpenses']");
            if (medicalexpenses != null)
            {
                medicalexpenses.InnerHtml = monthlyexpenses.MedicalExpenses.ToString();
            }

            var others = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Others']");
            if (others != null)
            {
                if (monthlyexpenses.Others != null || monthlyexpenses.OthersAmount != 0)
                {
                    others.InnerHtml = $"{monthlyexpenses.Others}, {monthlyexpenses.OthersAmount.ToString()}";
                }
                else
                {
                    others.InnerHtml = "";
                }
            }

            var transportation = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Transportation']");
            if (transportation != null)
            {
                transportation.InnerHtml = monthlyexpenses.Transportation.ToString();
            }

            var total = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Total']");
            if (total != null)
            {
                total.InnerHtml = monthlyexpenses.Total.ToString();
            }

            // UTILITIES
            switch (utilities.LightSource)
            {
                case "Electric":
                    var electriccheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Electriccheckbox']");
                    if (electriccheckbox != null)
                    {
                        string existingStyle = electriccheckbox.GetAttributeValue("style", "");
                        electriccheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    var electrictext = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Electrictext']");
                    if (electrictext != null)
                    {
                        electrictext.InnerHtml = utilities.LightSourceAmount.ToString();
                    }
                    break;
                case "Kerosene":
                    var kerosenecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Kerosenecheckbox']");
                    if (kerosenecheckbox != null)
                    {
                        string existingStyle = kerosenecheckbox.GetAttributeValue("style", "");
                        kerosenecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    var kerosenetext = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Kerosenetext']");
                    if (kerosenetext != null)
                    {
                        kerosenetext.InnerHtml = utilities.LightSourceAmount.ToString();
                    }
                    break;
                case "Candle":
                    var candlecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Candlecheckbox']");
                    if (candlecheckbox != null)
                    {
                        string existingStyle = candlecheckbox.GetAttributeValue("style", "");
                        candlecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    var candletext = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Candletext']");
                    if (candletext != null)
                    {
                        candletext.InnerHtml = utilities.LightSourceAmount.ToString();
                    }
                    break;
            }

            switch (utilities.FuelSource)
            {
                case "Gas":
                    var gascheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Gascheckbox']");
                    if (gascheckbox != null)
                    {
                        string existingStyle = gascheckbox.GetAttributeValue("style", "");
                        gascheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    var gastext = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Gastext']");
                    if (gastext != null)
                    {
                        gastext.InnerHtml = utilities.FuelSourceAmount.ToString();
                    }
                    break;
                case "Firewood":
                    var firewoodcheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Firewoodcheckbox']");
                    if (firewoodcheckbox != null)
                    {
                        string existingStyle = firewoodcheckbox.GetAttributeValue("style", "");
                        firewoodcheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    var firewoodtext = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Firewoodtext']");
                    if (firewoodtext != null)
                    {
                        firewoodtext.InnerHtml = utilities.FuelSourceAmount.ToString();
                    }
                    break;
                case "Charcoal":
                    var charcoalcheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Charcoalcheckbox']");
                    if (charcoalcheckbox != null)
                    {
                        string existingStyle = charcoalcheckbox.GetAttributeValue("style", "");
                        charcoalcheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    var charcoaltext = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Charcoaltext']");
                    if (charcoaltext != null)
                    {
                        charcoaltext.InnerHtml = utilities.FuelSourceAmount.ToString();
                    }
                    break;
            }

            switch (utilities.WaterSource)
            {
                case "Artesian Well":
                    var artesianwell = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='ArtesianWell']");
                    if (artesianwell != null)
                    {
                        string existingStyle = artesianwell.GetAttributeValue("style", "");
                        artesianwell.SetAttributeValue("style", existingStyle + "; border: 1px solid black; border-radius: 50%;");
                    }
                    break;
                case "Public":
                    var publiccheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Publiccheckbox']");
                    if (publiccheckbox != null)
                    {
                        string existingStyle = publiccheckbox.GetAttributeValue("style", "");
                        publiccheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Private":
                    var privatecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Privatecheckbox']");
                    if (privatecheckbox != null)
                    {
                        string existingStyle = privatecheckbox.GetAttributeValue("style", "");
                        privatecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Water District":
                    var waterdistrictcheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Waterdistrictcheckbox']");
                    if (waterdistrictcheckbox != null)
                    {
                        string existingStyle = waterdistrictcheckbox.GetAttributeValue("style", "");
                        waterdistrictcheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            // MEDICAL HISTORY
            var admittingdiagnosis = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Admittingdiagnosis']");
            if (admittingdiagnosis != null)
            {
                admittingdiagnosis.InnerHtml = medicalhistory.AdmittingDiagnosis;
            }

            var finaldiagnosis = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Finaldiagnosisupondischarge']");
            if (finaldiagnosis != null)
            {
                finaldiagnosis.InnerHtml = medicalhistory.FinalDiagnosis;
            }

            var durationofsymptoms = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Durationofthesymptoms']");
            if (durationofsymptoms != null)
            {
                durationofsymptoms.InnerHtml = medicalhistory.DurationSymptomsPriorAdmission;
            }

            var previoustreatment = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Previoustreatment']");
            if (previoustreatment != null)
            {
                previoustreatment.InnerHtml = medicalhistory.PreviousTreatmentDuration;
            }

            var treatmentplan = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Treatmentplan']");
            if (treatmentplan != null)
            {
                treatmentplan.InnerHtml = medicalhistory.TreatmentPlan;
            }

            var healthaccessibilityproblem = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Healthaccessibilityproblem']");
            if (healthaccessibilityproblem != null)
            {
                healthaccessibilityproblem.InnerHtml = medicalhistory.HealthAccessibilityProblems;
            }

            // CHILD HEALTH
            var num1 = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Num1']");
            if (num1 != null)
            {
                num1.InnerHtml = childhealth.OverallHealth;
            }

            switch(childhealth.HasHealthIssues)
            {
                case true:
                    var yescheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yesnum2checkbox']");
                    if (yescheckbox != null)
                    {
                        string existingStyle = yescheckbox.GetAttributeValue("style", "");
                        yescheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var nocheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nonum2checkbox']");
                    if (nocheckbox != null)
                    {
                        string existingStyle = nocheckbox.GetAttributeValue("style", "");
                        nocheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }
            var num2 = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Num2']");
            if (num2 != null)
            {
                num2.InnerHtml = childhealth.DescribeHealthIssues;
            }

            switch (childhealth.HasRecurrentConditions)
            {
                case true:
                    var yescheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yesnum3checkbox']");
                    if (yescheckbox != null)
                    {
                        string existingStyle = yescheckbox.GetAttributeValue("style", "");
                        yescheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var nocheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nonum3checkbox']");
                    if (nocheckbox != null)
                    {
                        string existingStyle = nocheckbox.GetAttributeValue("style", "");
                        nocheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }
            var num3 = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Num3']");
            if (num3 != null)
            {
                num3.InnerHtml = childhealth.DescribeRecurrentConditions;
            }

            switch (childhealth.HasEarTubes)
            {
                case true:
                    var yescheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yesnum4checkbox']");
                    if (yescheckbox != null)
                    {
                        string existingStyle = yescheckbox.GetAttributeValue("style", "");
                        yescheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var nocheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nonum4checkbox']");
                    if (nocheckbox != null)
                    {
                        string existingStyle = nocheckbox.GetAttributeValue("style", "");
                        nocheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            return htmlDoc.DocumentNode.OuterHtml; // Return updated HTML
        }

        public async Task<string> ModifyHtmlTemplateAsync_Page3(string htmlContent, int id)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using var context = new ApplicationDbContext(connectionString);

            var patient = await context.Patients.FindAsync(id);
            var diagnoses = await context.Diagnoses.Where(p => p.PatientID == id).ToListAsync();
            var medications = await context.Medications.Where(p => p.PatientID == id).ToListAsync();
            var hospitalizationhistory = await context.HospitalizationHistory.Where(p => p.PatientID == id).ToListAsync();
            var medicalscreenings = await context.MedicalScreenings.FirstOrDefaultAsync(c => c.PatientID == id);
            var primarycaredoctor = await context.PrimaryCareDoctor.FirstOrDefaultAsync(p => p.PatientID == id);
            var presentingproblems = await context.PresentingProblems.FirstOrDefaultAsync(p => p.PatientID == id);

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
                var medicalcondition = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Medicalconditions{i}']");
                if (medicalcondition != null)
                {
                    medicalcondition.InnerHtml = diagnosis.MedicalCondition;
                }

                switch (diagnosis.ReceivingTreatment)
                {
                    case true:
                        var currentlyreceivingtreatment = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Currentlyreceivingtreatment{i}']");
                        if (currentlyreceivingtreatment != null)
                        {
                            currentlyreceivingtreatment.InnerHtml = "Yes";
                        }
                        break;
                    case false:
                        var currentlyreceivingtreatment1 = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Currentlyreceivingtreatment{i}']");
                        if (currentlyreceivingtreatment1 != null)
                        {
                            currentlyreceivingtreatment1.InnerHtml = "No";
                        }
                        break;
                }

                var provider = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Provider{i}']");
                if (provider != null)
                {
                    provider.InnerHtml = diagnosis.TreatmentProvider;
                }

                switch (diagnosis.DoesCauseStressOrImpairment)
                {
                    case true:
                        var doescausestress = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Doesthisconditioncausestress{i}']");
                        if (doescausestress != null)
                        {
                            doescausestress.InnerHtml = "Yes";
                        }
                        break;
                    case false:
                        var doescausestress1 = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Doesthisconditioncausestress{i}']");
                        if (doescausestress1 != null)
                        {
                            doescausestress1.InnerHtml = "No";
                        }
                        break;
                }

                var whathaveyoufound = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Whathaveyoufound{i}']");
                if (whathaveyoufound != null)
                {
                    whathaveyoufound.InnerHtml = diagnosis.TreatmentHelp;
                }

                i++;
            }

            // MEDICATIONS
            switch (medications[0].DoesTakeAnyMedication)
            {
                case true:
                    var doestakeanymedication = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yesnum6checkbox']");
                    if (doestakeanymedication != null)
                    {
                        string existingStyle = doestakeanymedication.GetAttributeValue("style", "");
                        doestakeanymedication.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var doestakeanymedication1 = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nonum6checkbox']");
                    if (doestakeanymedication1 != null)
                    {
                        string existingStyle = doestakeanymedication1.GetAttributeValue("style", "");
                        doestakeanymedication1.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            int medCount = 1;
            foreach (var medication in medications)
            {
                var medicationname = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Medication{medCount}']");
                if (medicationname != null)
                {
                    medicationname.InnerHtml = medication.Medication;
                }

                var dosage = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Dosage{medCount}']");
                if (dosage != null)
                {
                    dosage.InnerHtml = medication.Dosage;
                }

                var frequency = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Frequency{medCount}']");
                if (frequency != null)
                {
                    frequency.InnerHtml = medication.Frequency;
                }

                var prescribedby = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Prescribedby{medCount}']");
                if (prescribedby != null)
                {
                    prescribedby.InnerHtml = medication.PrescribedBy;
                }

                var reasonformedication = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Reasonformedication{medCount}']");
                if (reasonformedication != null)
                {
                    reasonformedication.InnerHtml = medication.ReasonForMedication;
                }

                medCount++;
            }

            switch (medications[0].IsTakingMedicationAsPrescribed)
            {
                case true:
                    var istakingmedication = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yestakingmedicationcheckbox']");
                    if (istakingmedication != null)
                    {
                        string existingStyle = istakingmedication.GetAttributeValue("style", "");
                        istakingmedication.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var istakingmedication1 = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Notakingmedicationcheckbox']");
                    if (istakingmedication1 != null)
                    {
                        string existingStyle = istakingmedication1.GetAttributeValue("style", "");
                        istakingmedication1.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var istakingmedicationtext = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Isyourchildtakingmedication']");
            if (istakingmedicationtext != null)
            {
                istakingmedicationtext.InnerHtml = medications[0].DescribeTakingMedication;
            }

            var additionalinfo = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Additionalinformation']");
            if (additionalinfo != null)
            {
                additionalinfo.InnerHtml = medications[0].AdditionalInfo;
            }

            // HOSPITALIZATION HISTORY
            switch (hospitalizationhistory[0].HasSeriousAccidentOrIllness)
            {
                case true:
                    var hashospitalized = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yesnum7checkbox']");
                    if (hashospitalized != null)
                    {
                        string existingStyle = hashospitalized.GetAttributeValue("style", "");
                        hashospitalized.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var hashospitalized1 = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nonum7checkbox']");
                    if (hashospitalized1 != null)
                    {
                        string existingStyle = hashospitalized1.GetAttributeValue("style", "");
                        hashospitalized1.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            int hospCount = 1;
            foreach (var hospital in hospitalizationhistory)
            {
                var reason = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Reasonforprevious{hospCount}']");
                if (reason != null)
                {
                    reason.InnerHtml = hospital.Reason;
                }

                var date = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Datelocationofhospitalization{hospCount}']");
                if (date != null)
                {
                    date.InnerHtml = $"{hospital.Date.ToString()}, {hospital.Location}";
                }

                hospCount++;
            }

            // MEDICAL SCREENINGS
            switch (medicalscreenings.HasScreeningDone)
            {
                case true:
                    var hasscreeningdone = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Hasscreeningtest']");
                    if (hasscreeningdone != null)
                    {
                        hasscreeningdone.InnerHtml = "Yes";
                    }
                    break;
                case false:
                    var hasscreeningdone1 = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Hasscreeningtest']");
                    if (hasscreeningdone1 != null)
                    {
                        hasscreeningdone1.InnerHtml = "No";
                    }
                    break;
            }

            var hearingdate = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Hearingdate']");
            if (hearingdate != null)
            {
                hearingdate.InnerHtml = medicalscreenings.HearingTestDate.ToString();
            }

            var hearingoutcome = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Hearingoutcome']");
            if (hearingoutcome != null)
            {
                hearingoutcome.InnerHtml = medicalscreenings.HearingTestOutcome;
            }

            var visiondate = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Visiondate']");
            if (visiondate != null)
            {
                visiondate.InnerHtml = medicalscreenings.VisionTestDate.ToString();
            }

            var visionoutcome = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Visionoutcome']");
            if (visionoutcome != null)
            {
                visionoutcome.InnerHtml = medicalscreenings.VisionTestOutcome;
            }

            var speechdate = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Speechdate']");
            if (speechdate != null)
            {
                speechdate.InnerHtml = medicalscreenings.SpeechTestDate.ToString();
            }

            var speechoutcome = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Speechoutcome']");
            if (speechoutcome != null)
            {
                speechoutcome.InnerHtml = medicalscreenings.SpeechTestOutcome;
            }


            // PRIMARY CARE DOCTOR
            var primarydoctor = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Primarycaredoctor']");
            if (primarydoctor != null)
            {
                primarydoctor.InnerHtml = primarycaredoctor.DoctorName;
            }

            var facility = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Facility']");
            if (facility != null)
            {
                facility.InnerHtml = primarycaredoctor.Facility;
            }

            var phonenumber = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Phonenumber']");
            if (phonenumber != null)
            {
                phonenumber.InnerHtml = primarycaredoctor.PhoneNumber;
            }

            // PRESENTING PROBLEMS
            var presentingproblem = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Presentingproblem']");
            if (presentingproblem != null)
            {
                presentingproblem.InnerHtml = presentingproblems.PresentingProblem;
            }

            switch (presentingproblems.Severity)
            {
                case 1:
                    var one = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Onecheckbox']");
                    if (one != null)
                    {
                        string existingStyle = one.GetAttributeValue("style", "");
                        one.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case 2:
                    var two = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Twocheckbox']");
                    if (two != null)
                    {
                        string existingStyle = two.GetAttributeValue("style", "");
                        two.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case 3:
                    var three = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Threecheckbox']");
                    if (three != null)
                    {
                        string existingStyle = three.GetAttributeValue("style", "");
                        three.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case 4:
                    var four = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Fourcheckbox']");
                    if (four != null)
                    {
                        string existingStyle = four.GetAttributeValue("style", "");
                        four.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case 5:
                    var five = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Fivecheckbox']");
                    if (five != null)
                    {
                        string existingStyle = five.GetAttributeValue("style", "");
                        five.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case 6:
                    var six = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Sixcheckbox']");
                    if (six != null)
                    {
                        string existingStyle = six.GetAttributeValue("style", "");
                        six.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case 7:
                    var seven = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Sevencheckbox']");
                    if (seven != null)
                    {
                        string existingStyle = seven.GetAttributeValue("style", "");
                        seven.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case 8:
                    var eight = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Eightcheckbox']");
                    if (eight != null)
                    {
                        string existingStyle = eight.GetAttributeValue("style", "");
                        eight.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case 9:
                    var nine = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Ninecheckbox']");
                    if (nine != null)
                    {
                        string existingStyle = nine.GetAttributeValue("style", "");
                        nine.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case 10:
                    var ten = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Tencheckbox']");
                    if (ten != null)
                    {
                        string existingStyle = ten.GetAttributeValue("style", "");
                        ten.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;

            }

            switch (presentingproblems.ChangeInSleepPattern)
            {
                case "Sleeping more":
                    var sleepingmore = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Sleepingmorecheckbox']");
                    if (sleepingmore != null)
                    {
                        string existingStyle = sleepingmore.GetAttributeValue("style", "");
                        sleepingmore.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Sleeping less":
                    var sleepingless = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Sleepinglesscheckbox']");
                    if (sleepingless != null)
                    {
                        string existingStyle = sleepingless.GetAttributeValue("style", "");
                        sleepingless.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Difficulty falling asleep":
                    var difficultyfallingasleep = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Difficultyfallingasleepcheckbox']");
                    if (difficultyfallingasleep != null)
                    {
                        string existingStyle = difficultyfallingasleep.GetAttributeValue("style", "");
                        difficultyfallingasleep.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Difficulty staying asleep":
                    var difficultystayingasleep = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Difficultystayingasleepcheckbox']");
                    if (difficultystayingasleep != null)
                    {
                        string existingStyle = difficultystayingasleep.GetAttributeValue("style", "");
                        difficultystayingasleep.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Difficulty waking up":
                    var difficultywakingup = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Difficultywakingupcheckbox']");
                    if (difficultywakingup != null)
                    {
                        string existingStyle = difficultywakingup.GetAttributeValue("style", "");
                        difficultywakingup.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Difficulty staying awake":
                    var Difficultystayingawakecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Difficultystayingawakecheckbox']");
                    if (Difficultystayingawakecheckbox != null)
                    {
                        string existingStyle = Difficultystayingawakecheckbox.GetAttributeValue("style", "");
                        Difficultystayingawakecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            switch (presentingproblems.Concentration)
            {
                case "Decreased concentration":
                    var decreasedconcentration = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Decreasedconcentrationcheckbox']");
                    if (decreasedconcentration != null)
                    {
                        string existingStyle = decreasedconcentration.GetAttributeValue("style", "");
                        decreasedconcentration.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Increased or excessive concentration":
                    var increasedconcentration = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Increasedconcentrationcheckbox']");
                    if (increasedconcentration != null)
                    {
                        string existingStyle = increasedconcentration.GetAttributeValue("style", "");
                        increasedconcentration.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            switch (presentingproblems.ChangeInAppetite)
            {
                case "Increased appetite":
                    var increasedappetite = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Increasedappetitecheckbox']");
                    if (increasedappetite != null)
                    {
                        string existingStyle = increasedappetite.GetAttributeValue("style", "");
                        increasedappetite.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Decreased appetite":
                    var decreasedappetite = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Decreasedappetitecheckbox']");
                    if (decreasedappetite != null)
                    {
                        string existingStyle = decreasedappetite.GetAttributeValue("style", "");
                        decreasedappetite.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var increasedanxiety = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Increasedanxiety']");
            if (increasedanxiety != null)
            {
                increasedanxiety.InnerHtml = presentingproblems.IncreasedAnxiety;
            }

            var moodswings = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Moodswings']");
            if (moodswings != null)
            {
                moodswings.InnerHtml = presentingproblems.MoodSwings;
            }

            var behavioralchanges = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Behavioralproblems']");
            if (behavioralchanges != null)
            {
                behavioralchanges.InnerHtml = presentingproblems.BehavioralChanges;
            }

            switch (presentingproblems.Victimization)
            {
                case "Physical abuse":
                    var physicalabuse = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Physicalabusecheckbox']");
                    if (physicalabuse != null)
                    {
                        string existingStyle = physicalabuse.GetAttributeValue("style", "");
                        physicalabuse.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Sexual abuse":
                    var sexualabuse = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Sexualabusecheckbox']");
                    if (sexualabuse != null)
                    {
                        string existingStyle = sexualabuse.GetAttributeValue("style", "");
                        sexualabuse.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Psychological abuse":
                    var psychologicalabuse = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Psychologicalabusecheckbox']");
                    if (psychologicalabuse != null)
                    {
                        string existingStyle = psychologicalabuse.GetAttributeValue("style", "");
                        psychologicalabuse.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Robbery victim":
                    var robberyvictim = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Robberyvictimcheckbox']");
                    if (robberyvictim != null)
                    {
                        string existingStyle = robberyvictim.GetAttributeValue("style", "");
                        robberyvictim.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Assault victim":
                    var assaultvictim = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Assaultvictimcheckbox']");
                    if (assaultvictim != null)
                    {
                        string existingStyle = assaultvictim.GetAttributeValue("style", "");
                        assaultvictim.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Dating violence":
                    var datingviolence = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Datingviolencecheckbox']");
                    if (datingviolence != null)
                    {
                        string existingStyle = datingviolence.GetAttributeValue("style", "");
                        datingviolence.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Domestic violence":
                    var domesticviolence = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Domesticviolencecheckbox']");
                    if (domesticviolence != null)
                    {
                        string existingStyle = domesticviolence.GetAttributeValue("style", "");
                        domesticviolence.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Human trafficking":
                    var humantrafficking = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Humantraffickingcheckbox']");
                    if (humantrafficking != null)
                    {
                        string existingStyle = humantrafficking.GetAttributeValue("style", "");
                        humantrafficking.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "DUI/DWI crash":
                    var dui = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Duicrashcheckbox']");
                    if (dui != null)
                    {
                        string existingStyle = dui.GetAttributeValue("style", "");
                        dui.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Survivors of homicide victims":
                    var survivors = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Survivorscheckbox']");
                    if (survivors != null)
                    {
                        string existingStyle = survivors.GetAttributeValue("style", "");
                        survivors.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
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

            var otherconcerns = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Otherconcerns']");
            if (otherconcerns != null)
            {
                otherconcerns.InnerHtml = presentingproblems.DescribeOtherConcern;
            }

            return htmlDoc.DocumentNode.OuterHtml; // Return updated HTML
        }

        public async Task<string> ModifyHtmlTemplateAsync_Page4(string htmlContent, int id)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using var context = new ApplicationDbContext(connectionString);

            var patient = await context.Patients.FindAsync(id);
            var presentingproblems = await context.PresentingProblems.FirstOrDefaultAsync(p => p.PatientID == id);
            var recentlosses = await context.RecentLosses.FirstOrDefaultAsync(p => p.PatientID == id);
            var pregnancybirthhistory = await context.PregnancyBirthHistory.FirstOrDefaultAsync(p => p.PatientID == id);
            var developmentalhistory = await context.DevelopmentalHistory.FirstOrDefaultAsync(p => p.PatientID == id);
            var mentalhealthhistory = await context.MentalHealthHistory.Where(p => p.PatientID == id).ToListAsync();
            var familyhistory = await context.FamilyHistory.Where(p => p.PatientID == id).ToListAsync();
            var safetyconcerns = await context.SafetyConcerns.FirstOrDefaultAsync(p => p.PatientID == id);

            if (patient == null)
            {
                return string.Empty;
            }

            // USING HTMLAGILITYPACK
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // PRESENTING PROBLEMS
            switch (presentingproblems.DurationOfStress)
            {
                case "One week":
                    var oneweek = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Oneweekcheckbox']");
                    if (oneweek != null)
                    {
                        string existingStyle = oneweek.GetAttributeValue("style", "");
                        oneweek.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "One month":
                    var onemonth = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Onemonthcheckbox']");
                    if (onemonth != null)
                    {
                        string existingStyle = onemonth.GetAttributeValue("style", "");
                        onemonth.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "1-6 months":
                    var onetosixmonths = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Onetosixmonthscheckbox']");
                    if (onetosixmonths != null)
                    {
                        string existingStyle = onetosixmonths.GetAttributeValue("style", "");
                        onetosixmonths.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "6 months – 1 year":
                    var sixmonthstoyear = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Sixmonthstooneyearcheckbox']");
                    if (sixmonthstoyear != null)
                    {
                        string existingStyle = sixmonthstoyear.GetAttributeValue("style", "");
                        sixmonthstoyear.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Longer than one year":
                    var longerthanyear = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Longerthanoneyearcheckbox']");
                    if (longerthanyear != null)
                    {
                        string existingStyle = longerthanyear.GetAttributeValue("style", "");
                        longerthanyear.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            switch (presentingproblems.CopingLevel)
            {
                case 1:
                    var one = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Onecheckbox']");
                    if (one != null)
                    {
                        string existingStyle = one.GetAttributeValue("style", "");
                        one.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case 2:
                    var two = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Twocheckbox']");
                    if (two != null)
                    {
                        string existingStyle = two.GetAttributeValue("style", "");
                        two.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case 3:
                    var three = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Threecheckbox']");
                    if (three != null)
                    {
                        string existingStyle = three.GetAttributeValue("style", "");
                        three.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case 4:
                    var four = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Fourcheckbox']");
                    if (four != null)
                    {
                        string existingStyle = four.GetAttributeValue("style", "");
                        four.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case 5:
                    var five = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Fivecheckbox']");
                    if (five != null)
                    {
                        string existingStyle = five.GetAttributeValue("style", "");
                        five.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case 6:
                    var six = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Sixcheckbox']");
                    if (six != null)
                    {
                        string existingStyle = six.GetAttributeValue("style", "");
                        six.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case 7:
                    var seven = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Sevencheckbox']");
                    if (seven != null)
                    {
                        string existingStyle = seven.GetAttributeValue("style", "");
                        seven.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case 8:
                    var eight = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Eightcheckbox']");
                    if (eight != null)
                    {
                        string existingStyle = eight.GetAttributeValue("style", "");
                        eight.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case 9:
                    var nine = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Ninecheckbox']");
                    if (nine != null)
                    {
                        string existingStyle = nine.GetAttributeValue("style", "");
                        nine.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case 10:
                    var ten = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Tencheckbox']");
                    if (ten != null)
                    {
                        string existingStyle = ten.GetAttributeValue("style", "");
                        ten.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var otherfamilysituation = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Otherfamilysituation']");
            if (otherfamilysituation != null)
            {
                otherfamilysituation.InnerHtml = presentingproblems.OtherFamilySituation;
            }

            // RECENT LOSSES
            if (recentlosses.FamilyMemberLoss)
            {
                var familymemberloss = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Familymembercheckbox']");
                if (familymemberloss != null)
                {
                    string existingStyle = familymemberloss.GetAttributeValue("style", "");
                    familymemberloss.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (recentlosses.FriendLoss)
            {
                var friendloss = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Friendcheckbox']");
                if (friendloss != null)
                {
                    string existingStyle = friendloss.GetAttributeValue("style", "");
                    friendloss.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (recentlosses.HealthLoss)
            {
                var healthloss = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Healthcheckbox']");
                if (healthloss != null)
                {
                    string existingStyle = healthloss.GetAttributeValue("style", "");
                    healthloss.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (recentlosses.LifestyleLoss)
            {
                var lifestyleloss = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Lifestylecheckbox']");
                if (lifestyleloss != null)
                {
                    string existingStyle = lifestyleloss.GetAttributeValue("style", "");
                    lifestyleloss.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (recentlosses.JobLoss)
            {
                var jobloss = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Jobcheckbox']");
                if (jobloss != null)
                {
                    string existingStyle = jobloss.GetAttributeValue("style", "");
                    jobloss.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (recentlosses.IncomeLoss)
            {
                var incomeloss = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Incomecheckbox']");
                if (incomeloss != null)
                {
                    string existingStyle = incomeloss.GetAttributeValue("style", "");
                    incomeloss.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (recentlosses.HousingLoss)
            {
                var housingloss = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Housingcheckbox']");
                if (housingloss != null)
                {
                    string existingStyle = housingloss.GetAttributeValue("style", "");
                    housingloss.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (recentlosses.NoneLoss)
            {
                var noneloss = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nonecheckbox']");
                if (noneloss != null)
                {
                    string existingStyle = noneloss.GetAttributeValue("style", "");
                    noneloss.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            var who = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Whotext']");
            if (who != null)
            {
                who.InnerHtml = recentlosses.Name;
            }

            var when = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Whentext']");
            if (when != null)
            {
                when.InnerHtml = recentlosses.Date.ToString();
            }

            var natureofloss = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Naturetext']");
            if (natureofloss != null)
            {
                natureofloss.InnerHtml = recentlosses.NatureOfLoss;
            }

            var otherlosses = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Otherlossestext']");
            if (otherlosses != null)
            {
                otherlosses.InnerHtml = recentlosses.OtherLosses;
            }

            var additionalinforecentlosses = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Additionalinforecentlossestext']");
            if (additionalinforecentlosses != null)
            {
                additionalinforecentlosses.InnerHtml = recentlosses.AdditionalInfo;
            }

            // PREGNANCY BIRTH HISTORY
            switch (pregnancybirthhistory.HasPregnancyComplications)
            {
                case true:
                    var yesonecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yesonecheckbox']");
                    if (yesonecheckbox != null)
                    {
                        string existingStyle = yesonecheckbox.GetAttributeValue("style", "");
                        yesonecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var noonecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Noonecheckbox']");
                    if (noonecheckbox != null)
                    {
                        string existingStyle = noonecheckbox.GetAttributeValue("style", "");
                        noonecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var describepregnancycomplications = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Describepregnancycomplications']");
            if (describepregnancycomplications != null)
            {
                describepregnancycomplications.InnerHtml = pregnancybirthhistory.DescribePregnancyComplications;
            }

            switch (pregnancybirthhistory.IsFullTermBirth)
            {
                case true:
                    var fulltermbirth = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Fulltermbirthcheckbox']");
                    if (fulltermbirth != null)
                    {
                        string existingStyle = fulltermbirth.GetAttributeValue("style", "");
                        fulltermbirth.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var prematurebirth = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Prematurebirthcheckbox']");
                    if (prematurebirth != null)
                    {
                        string existingStyle = prematurebirth.GetAttributeValue("style", "");
                        prematurebirth.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            switch (pregnancybirthhistory.HasBirthComplications)
            {
                case true:
                    var yestwocheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yestwocheckbox']");
                    if (yestwocheckbox != null)
                    {
                        string existingStyle = yestwocheckbox.GetAttributeValue("style", "");
                        yestwocheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var notwocheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Notwocheckbox']");
                    if (notwocheckbox != null)
                    {
                        string existingStyle = notwocheckbox.GetAttributeValue("style", "");
                        notwocheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var describebirthcomplications = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Describebirthcomplications']");
            if (describebirthcomplications != null)
            {
                describebirthcomplications.InnerHtml = pregnancybirthhistory.DescribeBirthComplications;
            }

            switch (pregnancybirthhistory.HasConsumedDrugs)
            {
                case true:
                    var yesthreecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yesthreecheckbox']");
                    if (yesthreecheckbox != null)
                    {
                        string existingStyle = yesthreecheckbox.GetAttributeValue("style", "");
                        yesthreecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var nothreecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nothreecheckbox']");
                    if (nothreecheckbox != null)
                    {
                        string existingStyle = nothreecheckbox.GetAttributeValue("style", "");
                        nothreecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var birthweightlbs = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Birthweightlbs']");
            if (birthweightlbs != null)
            {
                birthweightlbs.InnerHtml = pregnancybirthhistory.BirthWeightLbs.ToString();
            }

            var birthweightoz = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Birthweightoz']");
            if (birthweightoz != null)
            {
                birthweightoz.InnerHtml = pregnancybirthhistory.BirthWeightOz.ToString();
            }

            var childhealthatbirth = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Childhealthatbirthtext']");
            if (childhealthatbirth != null)
            {
                childhealthatbirth.InnerHtml = pregnancybirthhistory.BirthHealth;
            }

            var lengthofhospitalstay = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Lengthofhospitalstaytext']");
            if (lengthofhospitalstay != null)
            {
                lengthofhospitalstay.InnerHtml = pregnancybirthhistory.LengthOfHospitalStay;
            }

            switch (pregnancybirthhistory.PostpartumDepression)
            {
                case true:
                    var yesfivecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yesfivecheckbox']");
                    if (yesfivecheckbox != null)
                    {
                        string existingStyle = yesfivecheckbox.GetAttributeValue("style", "");
                        yesfivecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var nofivecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nofivecheckbox']");
                    if (nofivecheckbox != null)
                    {
                        string existingStyle = nofivecheckbox.GetAttributeValue("style", "");
                        nofivecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            switch (pregnancybirthhistory.WasChildAdopted)
            {
                case true:
                    var yessixcheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yessixcheckbox']");
                    if (yessixcheckbox != null)
                    {
                        string existingStyle = yessixcheckbox.GetAttributeValue("style", "");
                        yessixcheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var nosixcheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nosixcheckbox']");
                    if (nosixcheckbox != null)
                    {
                        string existingStyle = nosixcheckbox.GetAttributeValue("style", "");
                        nosixcheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var adoptedage = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Adoptedagetext']");
            if (adoptedage != null)
            {
                adoptedage.InnerHtml = pregnancybirthhistory.ChildAdoptedAge.ToString();
            }

            switch (pregnancybirthhistory.AdoptionType)
            {
                case "Domestic adoption":
                    var domesticadoption = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Domesticadoptioncheckbox']");
                    if (domesticadoption != null)
                    {
                        string existingStyle = domesticadoption.GetAttributeValue("style", "");
                        domesticadoption.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "International adoption":
                    var internationaladoption = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Internationaladoptioncheckbox']");
                    if (internationaladoption != null)
                    {
                        string existingStyle = internationaladoption.GetAttributeValue("style", "");
                        internationaladoption.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Undocumented adoption":
                    var undocumentedadoption = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Undocumentedadoptioncheckbox']");
                    if (undocumentedadoption != null)
                    {
                        string existingStyle = undocumentedadoption.GetAttributeValue("style", "");
                        undocumentedadoption.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var adoptioncountry = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Adoptioncountrytext']");
            if (adoptioncountry != null)
            {
                adoptioncountry.InnerHtml = pregnancybirthhistory.AdoptionCountry;
            }

            // DEVELOPMENTAL HISTORY
            var rolledover = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Rolledovertext']");
            if (rolledover != null)
            {
                rolledover.InnerHtml = developmentalhistory.RolledOverAge.ToString();
            }

            var crawled = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Crawledtext']");
            if (crawled != null)
            {
                crawled.InnerHtml = developmentalhistory.CrawledAge.ToString();
            }

            var walked = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Walkedtext']");
            if (walked != null)
            {
                walked.InnerHtml = developmentalhistory.WalkedAge.ToString();
            }

            var talked = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Talkedtext']");
            if (talked != null)
            {
                talked.InnerHtml = developmentalhistory.TalkedAge.ToString();
            }

            var toilettrained = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Toilettrainedtext']");
            if (toilettrained != null)
            {
                toilettrained.InnerHtml = developmentalhistory.ToiletTrainedAge.ToString();
            }

            if (developmentalhistory.SpeechConcerns)
            {
                var speech = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Speechcheckbox']");
                if (speech != null)
                {
                    string existingStyle = speech.GetAttributeValue("style", "");
                    speech.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (developmentalhistory.MotorSkillsConcerns)
            {
                var motorskills = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Motorskillscheckbox']");
                if (motorskills != null)
                {
                    string existingStyle = motorskills.GetAttributeValue("style", "");
                    motorskills.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (developmentalhistory.CognitiveConcerns)
            {
                var cognitive = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Cognitivecheckbox']");
                if (cognitive != null)
                {
                    string existingStyle = cognitive.GetAttributeValue("style", "");
                    cognitive.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (developmentalhistory.SensoryConcerns)
            {
                var sensory = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Sensorycheckbox']");
                if (sensory != null)
                {
                    string existingStyle = sensory.GetAttributeValue("style", "");
                    sensory.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (developmentalhistory.BehavioralConcerns)
            {
                var behavioral = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Behavioralcheckbox']");
                if (behavioral != null)
                {
                    string existingStyle = behavioral.GetAttributeValue("style", "");
                    behavioral.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (developmentalhistory.EmotionalConcerns)
            {
                var emotional = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Emotionalcheckbox']");
                if (emotional != null)
                {
                    string existingStyle = emotional.GetAttributeValue("style", "");
                    emotional.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (developmentalhistory.SocialConcerns)
            {
                var social = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Socialcheckbox']");
                if (social != null)
                {
                    string existingStyle = social.GetAttributeValue("style", "");
                    social.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            switch (developmentalhistory.HasSignificantDisturbance)
            {
                case true:
                    var yessignificantissue = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yessignificantissuecheckbox']");
                    if (yessignificantissue != null)
                    {
                        string existingStyle = yessignificantissue.GetAttributeValue("style", "");
                        yessignificantissue.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var nosignificantissue = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nosignificantissuecheckbox']");
                    if (nosignificantissue != null)
                    {
                        string existingStyle = nosignificantissue.GetAttributeValue("style", "");
                        nosignificantissue.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var describesignificantissue = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Describesignificantissue']");
            if (describesignificantissue != null)
            {
                describesignificantissue.InnerHtml = developmentalhistory.DescribeSignificantDisturbance;
            }

            // MENTAL HEALTH HISTORY
            switch (mentalhealthhistory[0].HasReceivedCounseling)
            {
                case true:
                    var yesreceivedcounseling = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yesreceivedcounselingcheckbox']");
                    if (yesreceivedcounseling != null)
                    {
                        string existingStyle = yesreceivedcounseling.GetAttributeValue("style", "");
                        yesreceivedcounseling.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var noreceivedcounseling = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Noreceivedcounselingcheckbox']");
                    if (noreceivedcounseling != null)
                    {
                        string existingStyle = noreceivedcounseling.GetAttributeValue("style", "");
                        noreceivedcounseling.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            int mentalCount = 1;
            foreach (var mentalhealth in mentalhealthhistory)
            {
                var dateofservice = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Dateofservice{mentalCount}']");
                if (dateofservice != null)
                {
                    dateofservice.InnerHtml = mentalhealth.DateOfService.ToString();
                }

                var placeprovider = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Placeprovider{mentalCount}']");
                if (placeprovider != null)
                {
                    placeprovider.InnerHtml = mentalhealth.Provider;
                }

                var reasonfortreatment = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Reasonfortreatment{mentalCount}']");
                if (reasonfortreatment != null)
                {
                    reasonfortreatment.InnerHtml = mentalhealth.ReasonForTreatment;
                }

                string weretheserviceshelpfultext = mentalhealth.WereServicesHelpful ? "Yes" : "No";
                var weretheserviceshelpful = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Weretheserviceshelpful{mentalCount}']");
                if (weretheserviceshelpful != null)
                {
                    weretheserviceshelpful.InnerHtml = weretheserviceshelpfultext;
                }

                mentalCount++;
            }

            // FAMILY HISTORY
            foreach (var family in familyhistory)
            {
                int familyCount = family.IsSelf ? 1 : 2;

                string depressiontext = family.HasDepression ? "Yes" : "No";
                var depression = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Depression{familyCount}']");
                if (depression != null)
                {
                    depression.InnerHtml = depressiontext;
                }

                string anxietytext = family.HasAnxiety ? "Yes" : "No";
                var anxiety = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Anxiety{familyCount}']");
                if (anxiety != null)
                {
                    anxiety.InnerHtml = anxietytext;
                }

                string bipolarDisordertext = family.HasBipolarDisorder ? "Yes" : "No";
                var bipolarDisorder = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Bipolardisorder{familyCount}']");
                if (bipolarDisorder != null)
                {
                    bipolarDisorder.InnerHtml = bipolarDisordertext;
                }

                string schizophreniaText = family.HasSchizophrenia ? "Yes" : "No";
                var schizophrenia = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Schizophrenia{familyCount}']");
                if (schizophrenia != null)
                {
                    schizophrenia.InnerHtml = schizophreniaText;
                }

                string adhdaddText = family.HasADHD_ADD ? "Yes" : "No";
                var adhdadd = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Adhdadd{familyCount}']");
                if (adhdadd != null)
                {
                    adhdadd.InnerHtml = adhdaddText;
                }

                string traumahistoryText = family.HasTraumaHistory ? "Yes" : "No";
                var traumahistory = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Traumahistory{familyCount}']");
                if (traumahistory != null)
                {
                    traumahistory.InnerHtml = traumahistoryText;
                }

                string abusivebehaviorText = family.HasAbusiveBehavior ? "Yes" : "No";
                var abusivebehavior = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Abusivebehavior{familyCount}']");
                if (abusivebehavior != null)
                {
                    abusivebehavior.InnerHtml = abusivebehaviorText;
                }

                string alcoholabuseText = family.HasAlcoholAbuse ? "Yes" : "No";
                var alcoholabuse = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Alcoholabuse{familyCount}']");
                if (alcoholabuse != null)
                {
                    alcoholabuse.InnerHtml = alcoholabuseText;
                }

                string drugabuseText = family.HasDrugAbuse ? "Yes" : "No";
                var drugabuse = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Drugabuse{familyCount}']");
                if (drugabuse != null)
                {
                    drugabuse.InnerHtml = drugabuseText;
                }

                string incarcerationText = family.HasIncarceration ? "Yes" : "No";
                var incarceration = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Incarceration{familyCount}']");
                if (incarceration != null)
                {
                    incarceration.InnerHtml = incarcerationText;
                }

            }

            var additionalinfopatientfamily = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='Additionalinfopatientfamilytext']");
            if (additionalinfopatientfamily != null)
            {
                additionalinfopatientfamily.InnerHtml = familyhistory[0].AdditionalInfo;
            }

            // SAFETY CONCERNS
            switch (safetyconcerns.IsSuicidal)
            {
                case true:
                    var yespresentlysuicidal = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='yespresentlysuicidalcheckbox']");
                    if (yespresentlysuicidal != null)
                    {
                        string existingStyle = yespresentlysuicidal.GetAttributeValue("style", "");
                        yespresentlysuicidal.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var nopresentlysuicidal = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='nopresentlysuicidalcheckbox']");
                    if (nopresentlysuicidal != null)
                    {
                        string existingStyle = nopresentlysuicidal.GetAttributeValue("style", "");
                        nopresentlysuicidal.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var describesuicidal = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='ifyesexplainpresentlysuicidal']");
            if (describesuicidal != null)
            {
                describesuicidal.InnerHtml = safetyconcerns.DescribeSuicidal;
            }

            switch (safetyconcerns.HasAttemptedSuicide)
            {
                case true:
                    var yesattemptedsuicide = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='yeshasattemptedsuicidecheckbox']");
                    if (yesattemptedsuicide != null)
                    {
                        string existingStyle = yesattemptedsuicide.GetAttributeValue("style", "");
                        yesattemptedsuicide.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var noattemptedsuicide = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='nohasattemptedsuicidecheckbox']");
                    if (noattemptedsuicide != null)
                    {
                        string existingStyle = noattemptedsuicide.GetAttributeValue("style", "");
                        noattemptedsuicide.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var attemptedsuicide = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='ifyesexplainattemptedsuicide']");
            if (attemptedsuicide != null)
            {
                attemptedsuicide.InnerHtml = safetyconcerns.DescribeAttemptedSuicide;
            }

            switch (safetyconcerns.IsThereHistoryOfSuicide)
            {
                case true:
                    var yessuicidehistory = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='yessuicidehistorycheckbox']");
                    if (yessuicidehistory != null)
                    {
                        string existingStyle = yessuicidehistory.GetAttributeValue("style", "");
                        yessuicidehistory.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var nosuicidehistory = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='nosuicidehistorycheckbox']");
                    if (nosuicidehistory != null)
                    {
                        string existingStyle = nosuicidehistory.GetAttributeValue("style", "");
                        nosuicidehistory.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var describesuicidehistory = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='ifyesexplainsuicidehistory']");
            if (describesuicidehistory != null)
            {
                describesuicidehistory.InnerHtml = safetyconcerns.DescribeHistoryOfSuicide;
            }

            switch (safetyconcerns.HasSelfHarm)
            {
                case true:
                    var yesinflictedburns = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='yesinflictedburnscheckbox']");
                    if (yesinflictedburns != null)
                    {
                        string existingStyle = yesinflictedburns.GetAttributeValue("style", "");
                        yesinflictedburns.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var noinflictedburns = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='noinflictedburnscheckbox']");
                    if (noinflictedburns != null)
                    {
                        string existingStyle = noinflictedburns.GetAttributeValue("style", "");
                        noinflictedburns.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            return htmlDoc.DocumentNode.OuterHtml;
        }

        public async Task<string> ModifyHtmlTemplateAsync_Page5(string htmlContent, int id)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using var context = new ApplicationDbContext(connectionString);

            var patient = await context.Patients.FindAsync(id);
            var safetyconcerns = await context.SafetyConcerns.FirstOrDefaultAsync(p => p.PatientID == id);
            var currentfunctioning = await context.CurrentFunctioning.FirstOrDefaultAsync(p => p.PatientID == id);
            var parentchildrelationship = await context.ParentChildRelationship.FirstOrDefaultAsync(p => p.PatientID == id);

            if (patient == null)
            {
                return string.Empty;
            }

            // USING HTMLAGILITYPACK
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // SAFETY CONCERNS
            switch (safetyconcerns.IsHomicidal)
            {
                case true:
                    var yespresentlyhomicidal = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='yespresentlyhomicidalcheckbox']");
                    if (yespresentlyhomicidal != null)
                    {
                        string existingStyle = yespresentlyhomicidal.GetAttributeValue("style", "");
                        yespresentlyhomicidal.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var nopresentlyhomicidal = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='nopresentlyhomicidalcheckbox']");
                    if (nopresentlyhomicidal != null)
                    {
                        string existingStyle = nopresentlyhomicidal.GetAttributeValue("style", "");
                        nopresentlyhomicidal.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var describehomicidal = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='ifyesexplainpresentlyhomicidal']");
            if (describehomicidal != null)
            {
                describehomicidal.InnerHtml = safetyconcerns.DescribeHomicidal;
            }

            var safetyconcernsadditionalinfo = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='safetyconcernsadditionalinfo']");
            if (safetyconcernsadditionalinfo != null)
            {
                safetyconcernsadditionalinfo.InnerHtml = safetyconcerns.AdditionalInfo;
            }

            // CURRENT FUNCTIONING

            var describeconcerns = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='describeconcernscurrentfunctioning']");
            if (describeconcerns != null)
            {
                describeconcerns.InnerHtml = currentfunctioning.DescribeConcerns;
            }

            if (currentfunctioning.EatingConcerns)
            {
                var eatingconcerns = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='eatingcheckbox']");
                if (eatingconcerns != null)
                {
                    string existingStyle = eatingconcerns.GetAttributeValue("style", "");
                    eatingconcerns.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }
            if (currentfunctioning.HygieneConcerns)
            {
                var hygieneconcerns = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='hygienegroomingcheckbox']");
                if (hygieneconcerns != null)
                {
                    string existingStyle = hygieneconcerns.GetAttributeValue("style", "");
                    hygieneconcerns.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }
            if (currentfunctioning.SleepingConcerns)
            {
                var sleepingconcerns = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sleepingcheckbox']");
                if (sleepingconcerns != null)
                {
                    string existingStyle = sleepingconcerns.GetAttributeValue("style", "");
                    sleepingconcerns.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }
            if (currentfunctioning.ActivitiesConcerns)
            {
                var concerns = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='activitiesplaycheckbox']");
                if (concerns != null)
                {
                    string existingStyle = concerns.GetAttributeValue("style", "");
                    concerns.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }
            if (currentfunctioning.SocialRelationshipsConcerns)
            {
                var socialrelationshipsconcerns = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='socialrelationshipscheckbox']");
                if (socialrelationshipsconcerns != null)
                {
                    string existingStyle = socialrelationshipsconcerns.GetAttributeValue("style", "");
                    socialrelationshipsconcerns.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

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
            var parentingexperience = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='describeparentingyourchild']");
            if (parentingexperience != null)
            {
                parentingexperience.InnerHtml = parentchildrelationship.ParentingExperience;
            }

            var challenging = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='describemostchallenging']");
            if (challenging != null)
            {
                challenging.InnerHtml = parentchildrelationship.Challenges;
            }

            var disciplinemethods = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='describewhatkindofdiscipline']");
            if (disciplinemethods != null)
            {
                disciplinemethods.InnerHtml = parentchildrelationship.DisciplineMethods;
            }

            return htmlDoc.DocumentNode.OuterHtml; // Return updated HTML
        }

        public async Task<string> ModifyHtmlTemplateAsync_Page6(string htmlContent, int id)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using var context = new ApplicationDbContext(connectionString);

            var patient = await context.Patients.FindAsync(id); 
            var education = await context.Education.FirstOrDefaultAsync(p => p.PatientID == id);
            var employment = await context.Employment.FirstOrDefaultAsync(p => p.PatientID == id);
            var housing = await context.Housing.FirstOrDefaultAsync(p => p.PatientID == id);
            var fostercare = await context.FosterCare.FirstOrDefaultAsync(p => p.PatientID == id);
            var alcoholdrugassessment = await context.AlcoholDrugAssessment.FirstOrDefaultAsync(p => p.PatientID == id);

            if (patient == null)
            {
                return string.Empty;
            }

            // USING HTMLAGILITYPACK
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // EDUCATION
            switch (education.IsCurrentlyEnrolled)
            {
                case true:
                    var yescurrentlyenrolled = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yescurrentlyenrolledcheckbox']");
                    if (yescurrentlyenrolled != null)
                    {
                        string existingStyle = yescurrentlyenrolled.GetAttributeValue("style", "");
                        yescurrentlyenrolled.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var nocurrentlyenrolled = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nocurrentlyenrolledcheckbox']");
                    if (nocurrentlyenrolled != null)
                    {
                        string existingStyle = nocurrentlyenrolled.GetAttributeValue("style", "");
                        nocurrentlyenrolled.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var nameofschool = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nameofschool']");
            if (nameofschool != null)
            {
                nameofschool.InnerHtml = education.SchoolName;
            }

            var childgradelevel = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Childgrade']");
            if (childgradelevel != null)
            {
                childgradelevel.InnerHtml = education.ChildGradeLevel;
            }

            var summergradelevel = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Childsummergrade']");
            if (summergradelevel != null)
            {
                summergradelevel.InnerHtml = education.SummerGradeLevel;
            }

            var describechildattendance = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Describechildattendance']");
            if (describechildattendance != null)
            {
                describechildattendance.InnerHtml = education.DescribeChildAttendance;
            }

            switch (education.ChildAttendance)
            {
                case "Attending regularly":
                    var attendingregularly = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Attendingregularlycheckbox']");
                    if (attendingregularly != null)
                    {
                        string existingStyle = attendingregularly.GetAttributeValue("style", "");
                        attendingregularly.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Home-schooled":
                    var homeschooled = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Homeschooledcheckbox']");
                    if (homeschooled != null)
                    {
                        string existingStyle = homeschooled.GetAttributeValue("style", "");
                        homeschooled.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Some truancy":
                    var sometruancy = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Sometruancycheckbox']");
                    if (sometruancy != null)
                    {
                        string existingStyle = sometruancy.GetAttributeValue("style", "");
                        sometruancy.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Alternative school":
                    var alternativeschool = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Alternativeschoolcheckbox']");
                    if (alternativeschool != null)
                    {
                        string existingStyle = alternativeschool.GetAttributeValue("style", "");
                        alternativeschool.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Suspended":
                    var suspended = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Suspendedcheckbox']");
                    if (suspended != null)
                    {
                        string existingStyle = suspended.GetAttributeValue("style", "");
                        suspended.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Expelled":
                    var expelled = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Expelledcheckbox']");
                    if (expelled != null)
                    {
                        string existingStyle = expelled.GetAttributeValue("style", "");
                        expelled.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Dropped Out":
                    var droppedout = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Droppedoutcheckbox']");
                    if (droppedout != null)
                    {
                        string existingStyle = droppedout.GetAttributeValue("style", "");
                        droppedout.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Alternative Learning System (ALS)":
                    var alternativelearning = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Alternativelearningsystemcheckbox']");
                    if (alternativelearning != null)
                    {
                        string existingStyle = alternativelearning.GetAttributeValue("style", "");
                        alternativelearning.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var describechildachievements = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Describeachievements']");
            if (describechildachievements != null)
            {
                describechildachievements.InnerHtml = education.DescribeChildAchievements;
            }

            var describechildattitude = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Describeattitude']");
            if (describechildattitude != null)
            {
                describechildattitude.InnerHtml = education.DescribeChildAttitude;
            }

            switch (education.HasDisciplinaryIssues)
            {
                case true:
                    var yesdisciplinaryissues = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yesdisciplinaryissuescheckbox']");
                    if (yesdisciplinaryissues != null)
                    {
                        string existingStyle = yesdisciplinaryissues.GetAttributeValue("style", "");
                        yesdisciplinaryissues.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var nodisciplinaryissues = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nodisciplinaryissuescheckbox']");
                    if (nodisciplinaryissues != null)
                    {
                        string existingStyle = nodisciplinaryissues.GetAttributeValue("style", "");
                        nodisciplinaryissues.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }
            
            var describechilddisciplinary = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Describedisciplinaryissues']");
            if (describechilddisciplinary != null)
            {
                describechilddisciplinary.InnerHtml = education.DescribeDisciplinaryIssues;
            }

            if (education.HasSpecialEducation)
            {
                var specialeducation = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Specialeducationcheckbox']");
                if (specialeducation != null)
                {
                    string existingStyle = specialeducation.GetAttributeValue("style", "");
                    specialeducation.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
                var describespecialeducation = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Describespecialeducation']");
                if (describespecialeducation != null)
                {
                    describespecialeducation.InnerHtml = education.DescribeSpecialEducation;
                }
            }
            if (education.HasHomeStudy)
            {
                var homestudy = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Homestudycheckbox']");
                if (homestudy != null)
                {
                    string existingStyle = homestudy.GetAttributeValue("style", "");
                    homestudy.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
                var describehomestudy = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Describehomestudy']");
                if (describehomestudy != null)
                {
                    describehomestudy.InnerHtml = education.DescribeHomeStudy;
                }
            }
            if (education.HasDiagnosedLearningDisability)
            {
                var diagnosedlearningdisability = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Diagnosedlearningdisabilitycheckbox']");
                if (diagnosedlearningdisability != null)
                {
                    string existingStyle = diagnosedlearningdisability.GetAttributeValue("style", "");
                    diagnosedlearningdisability.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
                var describelearningdisability = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Describediagnosedlearningdisability']");
                if (describelearningdisability != null)
                {
                    describelearningdisability.InnerHtml = education.DescribeDiagnosedLearningDisability;
                }
            }
            if (education.HasSpecialServices)
            {
                var specialservices = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Receivingspecialservicescheckbox']");
                if (specialservices != null)
                {
                    string existingStyle = specialservices.GetAttributeValue("style", "");
                    specialservices.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
                var describespecialservices = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Describereceivingspecialservices']");
                if (describespecialservices != null)
                {
                    describespecialservices.InnerHtml = education.DescribeSpecialServices;
                }
            }

            // EMPLOYMENT
            switch (employment.IsCurrentlyEmployed)
            {
                case true:
                    var yescurrentlyemployed = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yescurrentlyemployedcheckbox']");
                    if (yescurrentlyemployed != null)
                    {
                        string existingStyle = yescurrentlyemployed.GetAttributeValue("style", "");
                        yescurrentlyemployed.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var nocurrentlyemployed = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nocurrentlyemployedcheckbox']");
                    if (nocurrentlyemployed != null)
                    {
                        string existingStyle = nocurrentlyemployed.GetAttributeValue("style", "");
                        nocurrentlyemployed.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var ifemployed = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Ifemployedwherearetheyworking']");
            if (ifemployed != null)
            {
                ifemployed.InnerHtml = employment.Location;
            }

            var howlong = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Howlongaretheyworking']");
            if (howlong != null)
            {
                howlong.InnerHtml = employment.JobDuration;
            }

            switch (employment.IsEnjoyingJob)
            {
                case true:
                    var yesenjoyingjob = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yesdoesenjoycheckbox']");
                    if (yesenjoyingjob != null)
                    {
                        string existingStyle = yesenjoyingjob.GetAttributeValue("style", "");
                        yesenjoyingjob.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var noenjoyingjob = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nodoesenjoycheckbox']");
                    if (noenjoyingjob != null)
                    {
                        string existingStyle = noenjoyingjob.GetAttributeValue("style", "");
                        noenjoyingjob.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            // HOUSING
            switch (housing.IsStable)
            {
                case true:
                    var yesstable = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Stablecheckbox']");
                    if (yesstable != null)
                    {
                        string existingStyle = yesstable.GetAttributeValue("style", "");
                        yesstable.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var nostable = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Unstablecheckbox']");
                    if (nostable != null)
                    {
                        string existingStyle = nostable.GetAttributeValue("style", "");
                        nostable.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var ifunstable = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Describeunstable']");
            if (ifunstable != null)
            {
                ifunstable.InnerHtml = housing.DescribeIfUnstable;
            }

            switch (housing.HousingType)
            {
                case "Parent/Guardian owns home":
                    var parentguardian = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Parentguardianownshomecheckbox']");
                    if (parentguardian != null)
                    {
                        string existingStyle = parentguardian.GetAttributeValue("style", "");
                        parentguardian.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Parent/Guardian rents home":
                    var parentguardian2 = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Parentguardianrentshomecheckbox']");
                    if (parentguardian2 != null)
                    {
                        string existingStyle = parentguardian2.GetAttributeValue("style", "");
                        parentguardian2.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Child and family live with relatives/friends (temporary)":
                    var childandfamily = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Temporarycheckbox']");
                    if (childandfamily != null)
                    {
                        string existingStyle = childandfamily.GetAttributeValue("style", "");
                        childandfamily.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Child and family live with relatives/friends (permanent)":
                    var childandfamily2 = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Permanentcheckbox']");
                    if (childandfamily2 != null)
                    {
                        string existingStyle = childandfamily2.GetAttributeValue("style", "");
                        childandfamily2.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Homeless":
                    var homeless = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Homelesscheckbox']");
                    if (homeless != null)
                    {
                        string existingStyle = homeless.GetAttributeValue("style", "");
                        homeless.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Transitional Housing":
                    var transitionalhousing = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Transitionalhousingcheckbox']");
                    if (transitionalhousing != null)
                    {
                        string existingStyle = transitionalhousing.GetAttributeValue("style", "");
                        transitionalhousing.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Emergency Shelter":
                    var emergencyshelter = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Emergencysheltercheckbox']");
                    if (emergencyshelter != null)
                    {
                        string existingStyle = emergencyshelter.GetAttributeValue("style", "");
                        emergencyshelter.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var howlongliving = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Howlonghaschildlivedincurrentsituation']");
            if (howlongliving != null)
            {
                howlongliving.InnerHtml = housing.DurationLivedInHouse;
            }

            var timesmoved = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Howmanytimeshaschildmoved']");
            if (timesmoved != null)
            {
                timesmoved.InnerHtml = housing.TimesMoved;
            }

            var housingadditionalinfo = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Housingadditionalinfo']");
            if (housingadditionalinfo != null)
            {
                housingadditionalinfo.InnerHtml = housing.AdditionalInfo;
            }

            // FOSTER CARE
            switch (fostercare.HasBeenFosterCared)
            {
                case "Yes":
                    var yes = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yeshasbeeninfostercarecheckbox']");
                    if (yes != null)
                    {
                        string existingStyle = yes.GetAttributeValue("style", "");
                        yes.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "No":
                    var no = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nohasbeeninfostercarecheckbox']");
                    if (no != null)
                    {
                        string existingStyle = no.GetAttributeValue("style", "");
                        no.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Unknown":
                    var unknown = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Unknownhasbeeninfostercarecheckbox']");
                    if (unknown != null)
                    {
                        string existingStyle = unknown.GetAttributeValue("style", "");
                        unknown.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var fosteragestart = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Fromage']");
            if (fosteragestart != null)
            {
                fosteragestart.InnerHtml = fostercare.FosterAgeEnd.ToString();
            }

            var fosterageend = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Toage']");
            if (fosterageend != null)
            {
                fosterageend.InnerHtml = fostercare.FosterAgeEnd.ToString();
            }

            var reason = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Describereasonfostercare']");
            if (reason != null)
            {
                reason.InnerHtml = fostercare.Reason;
            }

            switch (fostercare.PlacementType)
            {
                case "Familial Placement":
                    var familialplacement = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Familialplacementcheckbox']");
                    if (familialplacement != null)
                    {
                        string existingStyle = familialplacement.GetAttributeValue("style", "");
                        familialplacement.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Non-Familial Placement":
                    var nonfamilialplacement = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nonfamilialplacementcheckbox']");
                    if (nonfamilialplacement != null)
                    {
                        string existingStyle = nonfamilialplacement.GetAttributeValue("style", "");
                        nonfamilialplacement.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            switch (fostercare.CurrentStatus)
            {
                case "In-Care":
                    var incare = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Incarecheckbox']");
                    if (incare != null)
                    {
                        string existingStyle = incare.GetAttributeValue("style", "");
                        incare.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Out of Care":
                    var outofcare = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Outofcarecheckbox']");
                    if (outofcare != null)
                    {
                        string existingStyle = outofcare.GetAttributeValue("style", "");
                        outofcare.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            switch (fostercare.OutOfCareReason)
            {
                case "Adopted":
                    var adopted = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Adoptedcheckbox']");
                    if (adopted != null)
                    {
                        string existingStyle = adopted.GetAttributeValue("style", "");
                        adopted.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Returned to Home":
                    var returnedtohome = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Returnedtohomecheckbox']");
                    if (returnedtohome != null)
                    {
                        string existingStyle = returnedtohome.GetAttributeValue("style", "");
                        returnedtohome.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Emancipated":
                    var emancipated = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Emancipatedcheckbox']");
                    if (emancipated != null)
                    {
                        string existingStyle = emancipated.GetAttributeValue("style", "");
                        emancipated.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Ran away from care":
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

            // ALCOHOL/DRUG ASSESSMENT
            switch (alcoholdrugassessment.TobaccoUse)
            {
                case "Yes":
                    var tobacco = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yessmoketobaccocheckbox']");
                    if (tobacco != null)
                    {
                        string existingStyle = tobacco.GetAttributeValue("style", "");
                        tobacco.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "No":
                    var notobacco = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nosmoketobaccocheckbox']");
                    if (notobacco != null)
                    {
                        string existingStyle = notobacco.GetAttributeValue("style", "");
                        notobacco.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Do not know":
                    var dontknow = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Donotknowsmoketobaccocheckbox']");
                    if (dontknow != null)
                    {
                        string existingStyle = dontknow.GetAttributeValue("style", "");
                        dontknow.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            switch (alcoholdrugassessment.AlcoholUse)
            {
                case "Yes":
                    var alcohol = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yesusealcoholcheckbox']");
                    if (alcohol != null)
                    {
                        string existingStyle = alcohol.GetAttributeValue("style", "");
                        alcohol.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "No":
                    var noalcohol = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nousealcoholcheckbox']");
                    if (noalcohol != null)
                    {
                        string existingStyle = noalcohol.GetAttributeValue("style", "");
                        noalcohol.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Do not know":
                    var dontknowalcohol = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Donotknowusealcoholcheckbox']");
                    if (dontknowalcohol != null)
                    {
                        string existingStyle = dontknowalcohol.GetAttributeValue("style", "");
                        dontknowalcohol.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            switch (alcoholdrugassessment.RecreationalMedicationUse)
            {
                case "Yes":
                    var recreationalmedication = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yesrecreationallycheckbox']");
                    if (recreationalmedication != null)
                    {
                        string existingStyle = recreationalmedication.GetAttributeValue("style", "");
                        recreationalmedication.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "No":
                    var norecreationalmedication = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Norecreationallycheckbox']");
                    if (norecreationalmedication != null)
                    {
                        string existingStyle = norecreationalmedication.GetAttributeValue("style", "");
                        norecreationalmedication.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Do not know":
                    var dontknowrecreationalmedication = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Donotknowrecreationallycheckbox']");
                    if (dontknowrecreationalmedication != null)
                    {
                        string existingStyle = dontknowrecreationalmedication.GetAttributeValue("style", "");
                        dontknowrecreationalmedication.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            switch (alcoholdrugassessment.HasOverdosed)
            {
                case true:
                    var yesoverdose = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yesoverdosedcheckbox']");
                    if (yesoverdose != null)
                    {
                        string existingStyle = yesoverdose.GetAttributeValue("style", "");
                        yesoverdose.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var nooverdose = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nooverdosedcheckbox']");
                    if (nooverdose != null)
                    {
                        string existingStyle = nooverdose.GetAttributeValue("style", "");
                        nooverdose.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var ifoverdosed = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Describewhenoverdose']");
            if (ifoverdosed != null)
            {
                ifoverdosed.InnerHtml = alcoholdrugassessment.OverdoseDate;
            }

            return htmlDoc.DocumentNode.OuterHtml; // Return updated HTML

        }

        public async Task<string> ModifyHtmlTemplateAsync_Page7(string htmlContent, int id)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using var context = new ApplicationDbContext(connectionString);

            var patient = await context.Patients.FindAsync(id);
            var alcoholdrugassessment = await context.AlcoholDrugAssessment.FirstOrDefaultAsync(p => p.PatientID == id);
            var legalinvolvement = await context.LegalInvolvement.FirstOrDefaultAsync(p => p.PatientID == id);
            var historyofabuse = await context.HistoryOfAbuse.FirstOrDefaultAsync(p => p.PatientID == id);
            var historyofviolence = await context.HistoryOfViolence.FirstOrDefaultAsync(p => p.PatientID == id);

            if (patient == null)
            {
                return string.Empty;
            }

            // USING HTMLAGILITYPACK
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // ALCOHOL/DRUG ASSESSMENT
            switch (alcoholdrugassessment.HasAlcoholProblems)
            {
                case true:
                    var yes = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yesexperiencedproblemscheckbox']");
                    if (yes != null)
                    {
                        string existingStyle = yes.GetAttributeValue("style", "");
                        yes.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var no = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Noexperiencedproblemscheckbox']");
                    if (no != null)
                    {
                        string existingStyle = no.GetAttributeValue("style", "");
                        no.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            if (alcoholdrugassessment.LegalProblems)
            {
                var legalproblems = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Legalcheckbox']");
                if (legalproblems != null)
                {
                    string existingStyle = legalproblems.GetAttributeValue("style", "");
                    legalproblems.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }
            if (alcoholdrugassessment.SocialPeerProblems)
            {
                var socialpeerproblems = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Socialpeercheckbox']");
                if (socialpeerproblems != null)
                {
                    string existingStyle = socialpeerproblems.GetAttributeValue("style", "");
                    socialpeerproblems.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }
            if (alcoholdrugassessment.WorkProblems)
            {
                var workproblems = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Workcheckbox']");
                if (workproblems != null)
                {
                    string existingStyle = workproblems.GetAttributeValue("style", "");
                    workproblems.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }
            if (alcoholdrugassessment.FamilyProblems)
            {
                var familyproblems = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Familycheckbox']");
                if (familyproblems != null)
                {
                    string existingStyle = familyproblems.GetAttributeValue("style", "");
                    familyproblems.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }
            if (alcoholdrugassessment.FriendsProblems)
            {
                var friendsproblems = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Friendscheckbox']");
                if (friendsproblems != null)
                {
                    string existingStyle = friendsproblems.GetAttributeValue("style", "");
                    friendsproblems.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }
            if (alcoholdrugassessment.FinancialProblems)
            {
                var financialproblems = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Financialcheckbox']");
                if (financialproblems != null)
                {
                    string existingStyle = financialproblems.GetAttributeValue("style", "");
                    financialproblems.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            var describealcoholproblems = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Describealcoholproblems']");
            if (describealcoholproblems != null)
            {
                describealcoholproblems.InnerHtml = alcoholdrugassessment.DescribeProblems;
            }

            switch (alcoholdrugassessment.ContinuedUse)
            {
                case true:
                    var yescontinueduse = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yeshavecontinuedcheckbox']");
                    if (yescontinueduse != null)
                    {
                        string existingStyle = yescontinueduse.GetAttributeValue("style", "");
                        yescontinueduse.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var nocontinueduse = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nohavecontinuedcheckbox']");
                    if (nocontinueduse != null)
                    {
                        string existingStyle = nocontinueduse.GetAttributeValue("style", "");
                        nocontinueduse.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            // LEGAL INVOLVEMENT
            switch (legalinvolvement.HasCustodyCase)
            {
                case true:
                    var yescustody = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yescurrentcustodycheckbox']");
                    if (yescustody != null)
                    {
                        string existingStyle = yescustody.GetAttributeValue("style", "");
                        yescustody.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var nocustody = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nocurrentcustodycheckbox']");
                    if (nocustody != null)
                    {
                        string existingStyle = nocustody.GetAttributeValue("style", "");
                        nocustody.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var describecustody = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Describecurrentcustody']");
            if (describecustody != null)
            {
                describecustody.InnerHtml = legalinvolvement.DescribeCustodyCase;
            }

            switch (legalinvolvement.HasCPSInvolvement)
            {
                case "None":
                    var none = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nonecheckbox']");
                    if (none != null)
                    {
                        string existingStyle = none.GetAttributeValue("style", "");
                        none.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Past":
                    var past = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Pastcheckbox']");
                    if (past != null)
                    {
                        string existingStyle = past.GetAttributeValue("style", "");
                        past.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Current":
                    var current = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Currentcheckbox']");
                    if (current != null)
                    {
                        string existingStyle = current.GetAttributeValue("style", "");
                        current.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var describeCPS = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Describechildprotectiveservice']");
            if (describeCPS != null)
            {
                describeCPS.InnerHtml = legalinvolvement.DescribeCPSInvolvement;
            }

            switch (legalinvolvement.LegalStatus)
            {
                case "No Involvement":
                    var noinvolvement = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Noinvolvementcheckbox']");
                    if (noinvolvement != null)
                    {
                        string existingStyle = noinvolvement.GetAttributeValue("style", "");
                        noinvolvement.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Probation":
                    var probation = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Probationlengthcheckbox']");
                    if (probation != null)
                    {
                        string existingStyle = probation.GetAttributeValue("style", "");
                        probation.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    var probationlength = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Probationlength']");
                    if (probationlength != null)
                    {
                        probationlength.InnerHtml = legalinvolvement.ProbationParoleLength;
                    }
                    break;
                case "Parole":
                    var parole = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Parolelengthcheckbox']");
                    if (parole != null)
                    {
                        string existingStyle = parole.GetAttributeValue("style", "");
                        parole.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    var parolelength = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Parolelength']");
                    if (parolelength != null)
                    {
                        parolelength.InnerHtml = legalinvolvement.ProbationParoleLength;
                    }
                    break;
                case "Pending Charges":
                    var pendingcharges = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Chargespendingcheckbox']");
                    if (pendingcharges != null)
                    {
                        string existingStyle = pendingcharges.GetAttributeValue("style", "");
                        pendingcharges.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Prior Incarceration":
                    var priorincarceration = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Priorincarcerationcheckbox']");
                    if (priorincarceration != null)
                    {
                        string existingStyle = priorincarceration.GetAttributeValue("style", "");
                        priorincarceration.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case "Law Suit or other Court Proceeding":
                    var lawsuit = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Lawsuitcheckbox']");
                    if (lawsuit != null)
                    {
                        string existingStyle = lawsuit.GetAttributeValue("style", "");
                        lawsuit.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var charges = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Describecharges']");
            if (charges != null)
            {
                charges.InnerHtml = legalinvolvement.Charges;
            }

            var officername = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Officername']");
            if (officername != null)
            {
                officername.InnerHtml = legalinvolvement.OfficerName;
            }

            var officercontactnum = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Contactnum']");
            if (officercontactnum != null)
            {
                officercontactnum.InnerHtml = legalinvolvement.OfficerContactNum;
            }

            var legaladditionalinfo = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Legaladditionalinfo']");
            if (legaladditionalinfo != null)
            {
                legaladditionalinfo.InnerHtml = legalinvolvement.AdditionalInfo;
            }

            // HISTORY OF ABUSE
            switch (historyofabuse.HasBeenAbused)
            {
                case true:
                    var yesabuse = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yeshasbeenabusedcheckbox']");
                    if (yesabuse != null)
                    {
                        string existingStyle = yesabuse.GetAttributeValue("style", "");
                        yesabuse.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var noabuse = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nohasbeenabusedcheckbox']");
                    if (noabuse != null)
                    {
                        string existingStyle = noabuse.GetAttributeValue("style", "");
                        noabuse.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            if (historyofabuse.SexualAbuse)
            {
                var abuse = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Sexualabusecheckbox']");
                if (abuse != null)
                {
                    string existingStyle = abuse.GetAttributeValue("style", "");
                    abuse.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }

                var bywhom = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Bywhom1']");
                if (bywhom != null)
                {
                    bywhom.InnerHtml = historyofabuse.SexualAbuseByWhom;
                }

                var atwhatage = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Atwhatage1']");
                if (atwhatage != null)
                {
                    atwhatage.InnerHtml = historyofabuse.SexualAbuseAgeOfChild.ToString();
                }

                switch (historyofabuse.SexualAbuseReported)
                {
                    case true:
                        var yesreported = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yeswasitreported1checkbox']");
                        if (yesreported != null)
                        {
                            string existingStyle = yesreported.GetAttributeValue("style", "");
                            yesreported.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                    case false:
                        var noreported = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nowasitreported1checkbox']");
                        if (noreported != null)
                        {
                            string existingStyle = noreported.GetAttributeValue("style", "");
                            noreported.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                }
            }

            if (historyofabuse.PhysicalAbuse)
            {
                var abuse = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Physicalabusecheckbox']");
                if (abuse != null)
                {
                    string existingStyle = abuse.GetAttributeValue("style", "");
                    abuse.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }

                var bywhom = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Bywhom2']");
                if (bywhom != null)
                {
                    bywhom.InnerHtml = historyofabuse.PhysicalAbuseByWhom;
                }

                var atwhatage = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Atwhatage2']");
                if (atwhatage != null)
                {
                    atwhatage.InnerHtml = historyofabuse.PhysicalAbuseAgeOfChild.ToString();
                }

                switch (historyofabuse.PhysicalAbuseReported)
                {
                    case true:
                        var yesreported = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yeswasitreported2checkbox']");
                        if (yesreported != null)
                        {
                            string existingStyle = yesreported.GetAttributeValue("style", "");
                            yesreported.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                    case false:
                        var noreported = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nowasitreported2checkbox']");
                        if (noreported != null)
                        {
                            string existingStyle = noreported.GetAttributeValue("style", "");
                            noreported.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                }
            }

            if (historyofabuse.EmotionalAbuse)
            {
                var abuse = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Emotionalabusecheckbox']");
                if (abuse != null)
                {
                    string existingStyle = abuse.GetAttributeValue("style", "");
                    abuse.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
                var bywhom = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Bywhom3']");
                if (bywhom != null)
                {
                    bywhom.InnerHtml = historyofabuse.EmotionalAbuseByWhom;
                }
                var atwhatage = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Atwhatage3']");
                if (atwhatage != null)
                {
                    atwhatage.InnerHtml = historyofabuse.EmotionalAbuseAgeOfChild.ToString();
                }
                switch (historyofabuse.EmotionalAbuseReported)
                {
                    case true:
                        var yesreported = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yeswasitreported3checkbox']");
                        if (yesreported != null)
                        {
                            string existingStyle = yesreported.GetAttributeValue("style", "");
                            yesreported.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                    case false:
                        var noreported = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nowasitreported3checkbox']");
                        if (noreported != null)
                        {
                            string existingStyle = noreported.GetAttributeValue("style", "");
                            noreported.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                }
            }

            if (historyofabuse.VerbalAbuse)
            {
                var abuse = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Verbalabusecheckbox']");
                if (abuse != null)
                {
                    string existingStyle = abuse.GetAttributeValue("style", "");
                    abuse.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
                var bywhom = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Bywhom4']");
                if (bywhom != null)
                {
                    bywhom.InnerHtml = historyofabuse.VerbalAbuseByWhom;
                }
                var atwhatage = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Atwhatage4']");
                if (atwhatage != null)
                {
                    atwhatage.InnerHtml = historyofabuse.VerbalAbuseAgeOfChild.ToString();
                }
                switch (historyofabuse.VerbalAbuseReported)
                {
                    case true:
                        var yesreported = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yeswasitreported4checkbox']");
                        if (yesreported != null)
                        {
                            string existingStyle = yesreported.GetAttributeValue("style", "");
                            yesreported.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                    case false:
                        var noreported = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nowasitreported4checkbox']");
                        if (noreported != null)
                        {
                            string existingStyle = noreported.GetAttributeValue("style", "");
                            noreported.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                }
            }

            if (historyofabuse.AbandonedAbuse)
            {
                var abuse = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Abandonedneglectedabusecheckbox']");
                if (abuse != null)
                {
                    string existingStyle = abuse.GetAttributeValue("style", "");
                    abuse.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
                var bywhom = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Bywhom5']");
                if (bywhom != null)
                {
                    bywhom.InnerHtml = historyofabuse.AbandonedAbuseByWhom;
                }
                var atwhatage = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Atwhatage5']");
                if (atwhatage != null)
                {
                    atwhatage.InnerHtml = historyofabuse.AbandonedAbuseAgeOfChild.ToString();
                }
                switch(historyofabuse.AbandonedAbuseReported)
                {
                    case true:
                        var yesreported = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yeswasitreported5checkbox']");
                        if (yesreported != null)
                        {
                            string existingStyle = yesreported.GetAttributeValue("style", "");
                            yesreported.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                    case false:
                        var noreported = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nowasitreported5checkbox']");
                        if (noreported != null)
                        {
                            string existingStyle = noreported.GetAttributeValue("style", "");
                            noreported.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                }
            }

            if (historyofabuse.PsychologicalAbuse)
            {
                var abuse = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Psychologicalabusecheckbox']");
                if (abuse != null)
                {
                    string existingStyle = abuse.GetAttributeValue("style", "");
                    abuse.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
                var bywhom = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Bywhom6']");
                if (bywhom != null)
                {
                    bywhom.InnerHtml = historyofabuse.PsychologicalAbuseByWhom;
                }
                var atwhatage = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Atwhatage6']");
                if (atwhatage != null)
                {
                    atwhatage.InnerHtml = historyofabuse.PsychologicalAbuseAgeOfChild.ToString();
                }
                switch (historyofabuse.PsychologicalAbuseReported)
                {
                    case true:
                        var yesreported = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yeswasitreported6checkbox']");
                        if (yesreported != null)
                        {
                            string existingStyle = yesreported.GetAttributeValue("style", "");
                            yesreported.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                    case false:
                        var noreported = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nowasitreported6checkbox']");
                        if (noreported != null)
                        {
                            string existingStyle = noreported.GetAttributeValue("style", "");
                            noreported.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                }
            }

            switch (historyofabuse.VictimOfBullying)
            {
                case true:
                    var yesbully = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yesvictimofbullyingcheckbox']");
                    if (yesbully != null)
                    {
                        string existingStyle = yesbully.GetAttributeValue("style", "");
                        yesbully.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var nobully = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Novictimofbullyingcheckbox']");
                    if (nobully != null)
                    {
                        string existingStyle = nobully.GetAttributeValue("style", "");
                        nobully.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            switch (historyofabuse.SafetyConcerns)
            {
                case true:
                    var yessafety = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yessafetyconcernscheckbox']");
                    if (yessafety != null)
                    {
                        string existingStyle = yessafety.GetAttributeValue("style", "");
                        yessafety.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var nosafety = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nosafetyconcernscheckbox']");
                    if (nosafety != null)
                    {
                        string existingStyle = nosafety.GetAttributeValue("style", "");
                        nosafety.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var describesafetyconcerns = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Describesafetyconcerns']");
            if (describesafetyconcerns != null)
            {
                describesafetyconcerns.InnerHtml = historyofabuse.DescribeSafetyConcerns;
            }

            var historyofabuseadditionalinfo = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Historyofabuseadditionalinfo']");
            if (historyofabuseadditionalinfo != null)
            {
                historyofabuseadditionalinfo.InnerHtml = historyofabuse.AdditionalInfo;
            }

            // HISTORY OF VIOLENCE
            switch (historyofviolence.HasBeenAccused)
            {
                case true:
                    var yesaccused = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yeshasbeenaccusedcheckbox']");
                    if (yesaccused != null)
                    {
                        string existingStyle = yesaccused.GetAttributeValue("style", "");
                        yesaccused.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var noaccused = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Nohasbeenaccusedcheckbox']");
                    if (noaccused != null)
                    {
                        string existingStyle = noaccused.GetAttributeValue("style", "");
                        noaccused.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            if (historyofviolence.SexualAbuse)
            {
                var towhom = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Accusedtowhom1']");
                if (towhom != null)
                {
                    towhom.InnerHtml = historyofviolence.SexualAbuseToWhom;
                }

                var atwhatage = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Accusedatwhatage1']");
                if (atwhatage != null)
                {
                    atwhatage.InnerHtml = historyofviolence.SexualAbuseAgeOfChild.ToString();
                }

                switch (historyofviolence.SexualAbuseReported)
                {
                    case true:
                        var yesreported = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Accusedyeswasitreported1checkbox']");
                        if (yesreported != null)
                        {
                            string existingStyle = yesreported.GetAttributeValue("style", "");
                            yesreported.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                    case false:
                        var noreported = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Accusednowasitreported1checkbox']");
                        if (noreported != null)
                        {
                            string existingStyle = noreported.GetAttributeValue("style", "");
                            noreported.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                }
            }

            if (historyofviolence.PhysicalAbuse)
            {
                var towhom = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Accusedtowhom2']");
                if (towhom != null)
                {
                    towhom.InnerHtml = historyofviolence.PhysicalAbuseToWhom;
                }
                var atwhatage = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Accusedatwhatage2']");
                if (atwhatage != null)
                {
                    atwhatage.InnerHtml = historyofviolence.PhysicalAbuseAgeOfChild.ToString();
                }
                switch (historyofviolence.PhysicalAbuseReported)
                {
                    case true:
                        var yesreported = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Accusedyeswasitreported2checkbox']");
                        if (yesreported != null)
                        {
                            string existingStyle = yesreported.GetAttributeValue("style", "");
                            yesreported.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                    case false:
                        var noreported = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Accusednowasitreported2checkbox']");
                        if (noreported != null)
                        {
                            string existingStyle = noreported.GetAttributeValue("style", "");
                            noreported.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                }
            }

            if (historyofviolence.EmotionalAbuse)
            {
                var towhom = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Accusedtowhom3']");
                if (towhom != null)
                {
                    towhom.InnerHtml = historyofviolence.EmotionalAbuseToWhom;
                }
                var atwhatage = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Accusedatwhatage3']");
                if (atwhatage != null)
                {
                    atwhatage.InnerHtml = historyofviolence.EmotionalAbuseAgeOfChild.ToString();
                }
                switch (historyofviolence.EmotionalAbuseReported)
                {
                    case true:
                        var yesreported = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Accusedyeswasitreported3checkbox']");
                        if (yesreported != null)
                        {
                            string existingStyle = yesreported.GetAttributeValue("style", "");
                            yesreported.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                    case false:
                        var noreported = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Accusednowasitreported3checkbox']");
                        if (noreported != null)
                        {
                            string existingStyle = noreported.GetAttributeValue("style", "");
                            noreported.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                }
            }

            if (historyofviolence.VerbalAbuse)
            {
                var towhom = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Accusedtowhom4']");
                if (towhom != null)
                {
                    towhom.InnerHtml = historyofviolence.VerbalAbuseToWhom;
                }
                var atwhatage = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Accusedatwhatage4']");
                if (atwhatage != null)
                {
                    atwhatage.InnerHtml = historyofviolence.VerbalAbuseAgeOfChild.ToString();
                }
                switch (historyofviolence.VerbalAbuseReported)
                {
                    case true:
                        var yesreported = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Accusedyeswasitreported4checkbox']");
                        if (yesreported != null)
                        {
                            string existingStyle = yesreported.GetAttributeValue("style", "");
                            yesreported.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                    case false:
                        var noreported = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Accusednowasitreported4checkbox']");
                        if (noreported != null)
                        {
                            string existingStyle = noreported.GetAttributeValue("style", "");
                            noreported.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                }
            }

            if (historyofviolence.AbandonedAbuse)
            {
                var towhom = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Accusedtowhom5']");
                if (towhom != null)
                {
                    towhom.InnerHtml = historyofviolence.AbandonedAbuseToWhom;
                }
                var atwhatage = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Accusedatwhatage5']");
                if (atwhatage != null)
                {
                    atwhatage.InnerHtml = historyofviolence.AbandonedAbuseAgeOfChild.ToString();
                }
                switch (historyofviolence.AbandonedAbuseReported)
                {
                    case true:
                        var yesreported = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Accusedyeswasitreported5checkbox']");
                        if (yesreported != null)
                        {
                            string existingStyle = yesreported.GetAttributeValue("style", "");
                            yesreported.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                    case false:
                        var noreported = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Accusednowasitreported5checkbox']");
                        if (noreported != null)
                        {
                            string existingStyle = noreported.GetAttributeValue("style", "");
                            noreported.SetAttributeValue("style", existingStyle + "; background-color: black;");
                        }
                        break;
                }
            }

            switch (historyofviolence.Bullying)
            {
                case true:
                    var yesbully = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Yesknowntobullycheckbox']");
                    if (yesbully != null)
                    {
                        string existingStyle = yesbully.GetAttributeValue("style", "");
                        yesbully.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
                case false:
                    var nobully = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Noknowntobullycheckbox']");
                    if (nobully != null)
                    {
                        string existingStyle = nobully.GetAttributeValue("style", "");
                        nobully.SetAttributeValue("style", existingStyle + "; background-color: black;");
                    }
                    break;
            }

            var historyofviolenceadditionalinfo = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Historyofviolenceadditionalinfo']");
            if (historyofviolenceadditionalinfo != null)
            {
                historyofviolenceadditionalinfo.InnerHtml = historyofviolence.AdditionalInfo;
            }


            return htmlDoc.DocumentNode.OuterHtml; // Return updated HTML
        }

        public async Task<string> ModifyHtmlTemplateAsync_Page8(string htmlContent, int id)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using var context = new ApplicationDbContext(connectionString);

            var patient = await context.Patients.FindAsync(id);
            var strengthsresources = await context.StrengthsResources.FirstOrDefaultAsync(p => p.PatientID == id);
            var goals = await context.Goals.FirstOrDefaultAsync(p => p.PatientID == id);
            var assessments = await context.Assessments.FirstOrDefaultAsync(p => p.PatientID == id);

            if (patient == null)
            {
                return string.Empty;
            }

            // USING HTMLAGILITYPACK
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // STRENGTHS AND RESOURCES
            var strengths = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Strengths']");
            if (strengths != null)
            {
                strengths.InnerHtml = strengthsresources.Strengths;
            }

            var limitations = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Limitations']");
            if (limitations != null)
            {
                limitations.InnerHtml = strengthsresources.Limitations;
            }

            var resources = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Resources']");
            if (resources != null)
            {
                resources.InnerHtml = strengthsresources.Resources;
            }

            var experiences = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Experiences']");
            if (experiences != null)
            {
                experiences.InnerHtml = strengthsresources.Experiences;
            }

            var alreadydoing = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Alreadydoing']");
            if (alreadydoing != null)
            {
                alreadydoing.InnerHtml = strengthsresources.AlreadyDoing;
            }

            if (strengthsresources.ParentsSupport)
            {
                var parentsupport = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Parentscheckbox']");
                if (parentsupport != null)
                {
                    string existingStyle = parentsupport.GetAttributeValue("style", "");
                    parentsupport.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (strengthsresources.PartnerSupport)
            {
                var partnersupport = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Boyfriendgirlfriendcheckbox']");
                if (partnersupport != null)
                {
                    string existingStyle = partnersupport.GetAttributeValue("style", "");
                    partnersupport.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (strengthsresources.SiblingsSupport)
            {
                var siblingsupport = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Siblingscheckbox']");
                if (siblingsupport != null)
                {
                    string existingStyle = siblingsupport.GetAttributeValue("style", "");
                    siblingsupport.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (strengthsresources.ExtendedFamilySupport)
            {
                var extendedfamilysupport = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Extendedfamilycheckbox']");
                if (extendedfamilysupport != null)
                {
                    string existingStyle = extendedfamilysupport.GetAttributeValue("style", "");
                    extendedfamilysupport.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (strengthsresources.FriendsSupport)
            {
                var friendssupport = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Friendscheckbox']");
                if (friendssupport != null)
                {
                    string existingStyle = friendssupport.GetAttributeValue("style", "");
                    friendssupport.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (strengthsresources.NeighborsSupport)
            {
                var neighborssupport = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Neighborscheckbox']");
                if (neighborssupport != null)
                {
                    string existingStyle = neighborssupport.GetAttributeValue("style", "");
                    neighborssupport.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (strengthsresources.SchoolStaffSupport)
            {
                var schoolstaffsupport = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Schoolstaffcheckbox']");
                if (schoolstaffsupport != null)
                {
                    string existingStyle = schoolstaffsupport.GetAttributeValue("style", "");
                    schoolstaffsupport.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (strengthsresources.ChurchSupport)
            {
                var churchsupport = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Churchcheckbox']");
                if (churchsupport != null)
                {
                    string existingStyle = churchsupport.GetAttributeValue("style", "");
                    churchsupport.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (strengthsresources.PastorSupport)
            {
                var pastorsupport = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Pastorcheckbox']");
                if (pastorsupport != null)
                {
                    string existingStyle = pastorsupport.GetAttributeValue("style", "");
                    pastorsupport.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (strengthsresources.TherapistSupport)
            {
                var therapistsupport = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Therapistcheckbox']");
                if (therapistsupport != null)
                {
                    string existingStyle = therapistsupport.GetAttributeValue("style", "");
                    therapistsupport.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (strengthsresources.GroupSupport)
            {
                var groupsupport = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Groupcheckbox']");
                if (groupsupport != null)
                {
                    string existingStyle = groupsupport.GetAttributeValue("style", "");
                    groupsupport.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (strengthsresources.CommunityServiceSupport)
            {
                var communityservice = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Communityservicecheckbox']");
                if (communityservice != null)
                {
                    string existingStyle = communityservice.GetAttributeValue("style", "");
                    communityservice.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (strengthsresources.DoctorSupport)
            {
                var doctor = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Doctorcheckbox']");
                if (doctor != null)
                {
                    string existingStyle = doctor.GetAttributeValue("style", "");
                    doctor.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }
            }

            if (strengthsresources.OthersSupport)
            {
                var otherssupport = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Othercheckbox']");
                if (otherssupport != null)
                {
                    string existingStyle = otherssupport.GetAttributeValue("style", "");
                    otherssupport.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }

                var others = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Otherssix']");
                if (others != null)
                {
                    others.InnerHtml = strengthsresources.Others;
                }
            }

            // GOALS
            var currentneeds = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Biggestneedrightnow']");
            if (currentneeds != null)
            {
                currentneeds.InnerHtml = goals.CurrentNeeds;
            }

            var hopetogain = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Hopetogain']");
            if (hopetogain != null)
            {
                hopetogain.InnerHtml = goals.HopeToGain;
            }

            var goal1 = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Goal1text']");
            if (goal1 != null)
            {
                goal1.InnerHtml = goals.Goal1;
            }

            var goal2 = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Goal2text']");
            if (goal2 != null)
            {
                goal2.InnerHtml = goals.Goal2;
            }

            var goal3 = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Goal3text']");
            if (goal3 != null)
            {
                goal3.InnerHtml = goals.Goal3;
            }

            var additionalinfo = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Goalsadditionalinfo']");
            if (additionalinfo != null)
            {
                additionalinfo.InnerHtml = goals.AdditionalInfo;
            }

            // ASSESSMENTS
            var assessmentstatement = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Assessmentstatement']");
            if (assessmentstatement != null)
            {
                assessmentstatement.InnerHtml = assessments.AssessmentStatement;
            }


            return htmlDoc.DocumentNode.OuterHtml; // Return updated HTML
        }
    }
}
