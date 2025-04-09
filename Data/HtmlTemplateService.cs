using HtmlAgilityPack;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

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

            return htmlDoc.DocumentNode.OuterHtml; // Return updated HTML
        }
    }
}
