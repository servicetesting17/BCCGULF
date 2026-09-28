function toggleRejectionRemark(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();

    var statusAttr = formContext.getAttribute("bcc_status");
    var remarkAttr = formContext.getAttribute("bcc_rejectionremark");
    var remarkCtrl = formContext.getControl("bcc_rejectionremark");

    if (!statusAttr || !remarkAttr || !remarkCtrl) {
        return;
    }

    var statusValue = statusAttr.getValue();

    if (statusValue === 755700004) {
        // Show and make mandatory
        remarkCtrl.setVisible(true);
        remarkAttr.setRequiredLevel("required");
    } else {
        // Hide and remove mandatory
        remarkCtrl.setVisible(false);
        remarkAttr.setRequiredLevel("none");
        remarkAttr.setValue(null); // optional: clear value
    }
}


async function createContact_OnSaves(executionContext) {
    debugger;
    try {
        var formContext = executionContext.getFormContext();

        //  Prevent duplicate contact creation
        var contactLookup = formContext.getAttribute("bcc_contact");
        if (contactLookup && contactLookup.getValue()) {
            return;
        }

        //  Safe getter
        function getValue(field) {
            debugger;
            var attr = formContext.getAttribute(field);
            return attr ? attr.getValue() : null;
        }

        var firstName = getValue("bcc_name");
        var lastName = getValue("bcc_lastname");

        if (!firstName || !lastName) {
            return;
        }

        //  Create Contact payload
        var contact = {
            firstname: firstName,
            lastname: lastName,
            bcc_designationaspervisa: getValue("bcc_designationaspervisa"),
            bcc_dateofbirth: getValue("bcc_dateofbirth"),
            bcc_labourcardexpiry: getValue("bcc_labourcardexpiry"),
            bcc_placeofbirth: getValue("bcc_placeofbirth"),
            bcc_molnumber: getValue("bcc_molnumber"),
            bcc_uidno: getValue("bcc_uidno"),
            bcc_workmanscomp: getValue("bcc_workmanscomp"),
            bcc_probationcompletiondate: getValue("bcc_probationcompletiondate"),
            bcc_emiratesid: getValue("bcc_emiratesid"),
            bcc_department: getValue("bcc_department"),
            bcc_ppexpirydate: getValue("bcc_ppexpirydate"),
            bcc_visano: getValue("bcc_visano"),
            bcc_designationincompany: getValue("bcc_designationincompany"),
            bcc_visaexpirydate: getValue("bcc_visaexpirydate"),
            bcc_labourcardno: getValue("bcc_labourcardno"),
            bcc_passportno: getValue("bcc_passportno"),
            bcc_nationality_: getValue("bcc_nationality_"), // TEXT FIELD
            bcc_placeofissue: getValue("bcc_placeofissue"),
            bcc_medicalinsurance: getValue("bcc_medicalinsurance"),
            bcc_joiningdate: getValue("bcc_joiningdate")
        };

        //  Create Contact
        var result = await Xrm.WebApi.createRecord("contact", contact);

        // Set Contact lookup on form
        var contactId = "{" + result.id + "}";

        //  Set lookup
        contactLookup.setValue([{
            id: contactId,
            name: firstName + " " + lastName,
            entityType: "contact"
        }]);

        contactLookup.setSubmitMode("always");

        console.log(" Contact created & lookup set");



    } catch (error) {
        console.error(" Error:", error.message);
    }
}








var BCC = BCC || {};

BCC.JobOffer = {

    populateFromJobApplication: function (executionContext) {
        debugger;

        var formContext = executionContext.getFormContext();

        var jobAppLookup = formContext.getAttribute("bcc_jobapplication").getValue();

        // If lookup is empty â†’ clear fields
        if (!jobAppLookup || jobAppLookup.length === 0) {
            formContext.getAttribute("bcc_basic").setValue(null);
            formContext.getAttribute("bcc_totalsalary").setValue(null);
            formContext.getAttribute("bcc_allowance").setValue(null);
            return;
        }

        var jobAppId = jobAppLookup[0].id.replace("{", "").replace("}", "");

        Xrm.WebApi.retrieveRecord(
            "bcc_jobapplication",
            jobAppId,
            "?$select=bcc_basic,bcc_totalsalary,bcc_allowance"
        ).then(
            function (result) {
                debugger;

                if (formContext.getAttribute("bcc_basic"))
                    formContext.getAttribute("bcc_basic").setValue(result.bcc_basic);

                if (formContext.getAttribute("bcc_totalsalary"))
                    formContext.getAttribute("bcc_totalsalary").setValue(result.bcc_totalsalary);

                if (formContext.getAttribute("bcc_allowance"))
                    formContext.getAttribute("bcc_allowance").setValue(result.bcc_allowance);

                console.log(" Job Application data copied successfully");
            },
            function (error) {
                debugger;
                console.error(" Error retrieving Job Application:", error.message);
            }
        );
    }
};







var BCC = BCC || {};

BCC.BPFAA = {

    switchFormOnStageChange: function (executionContext) {
        debugger;

        var formContext = executionContext.getFormContext();
        var process = formContext.data.process;

        if (!process) return;

        process.addOnStageChange(function () {
            debugger;

            var stage = process.getActiveStage();
            if (!stage) return;

            //  Check correct stage
            if (stage.getName() === "Apply Visa Form") {

                var targetFormId = "6b56f616-24e4-f011-8406-70a8a522b0a0";

                //  IMPORTANT: Delay required
                setTimeout(function () {
                    debugger;

                    Xrm.Navigation.openForm({
                        entityName: formContext.data.entity.getEntityName(),
                        entityId: formContext.data.entity.getId(),
                        formId: targetFormId
                    });

                }, 1500); //  don't reduce
            }
        });
    }
};






var BCC = BCC || {};

BCC.BPFValidation = {

    onLoad: function (executionContext) {
        debugger;
        var formContext = executionContext.getFormContext();

        var dobAttr = formContext.getAttribute("bcc_dateofbirth");
        if (!dobAttr) return;

        // Attach onchange
        dobAttr.addOnChange(function () {
            debugger;
            BCC.BPFValidation.validateDOB(formContext);
        });
    },

    validateDOB: function (formContext) {
        debugger;
        var dobAttr = formContext.getAttribute("bcc_dateofbirth");
        if (!dobAttr || !dobAttr.getValue()) return;

        var selectedDate = new Date(dobAttr.getValue());
        var today = new Date();

        // Remove time part
        selectedDate.setHours(0, 0, 0, 0);
        today.setHours(0, 0, 0, 0);

        if (selectedDate > today) {
            alert(" Future date is not allowed. Please select today or past date.");

            dobAttr.setValue(null);
            dobAttr.setSubmitMode("always");
        }
    }
};











var BCC = BCC || {};

BCC.BPFAMOL = {

    onLoad: function (executionContext) {
        debugger;

        var formContext = executionContext.getFormContext();

        if (!formContext.data.process) return;

        formContext.data.process.addOnStageChange(function () {
            debugger;

            var stage = formContext.data.process.getActiveStage();
            if (!stage) return;

            var stageName = stage.getName();

            //  EXACT stage name
            if (stageName === "MOL Verfication") {

                var targetFormId = "ea396a4e-31e6-f011-8406-70a8a52030e9";

                //  Delay is VERY important
                setTimeout(function () {
                    debugger;

                    formContext.ui.formSelector.items.forEach(function (item) {
                        debugger;

                        if (item.getId().toLowerCase() === targetFormId.toLowerCase()) {
                            item.navigate();
                        }

                    });

                }, 800); // <do NOT remove this
            }
        });
    }
};








var BCC = BCC || {};

BCC.BPFAMOLLEV = {

    onLoad: function (executionContext) {
        debugger;
        var formContext = executionContext.getFormContext();

        if (!formContext.data.process) return;

        formContext.data.process.addOnStageChange(function () {
            debugger;
            BCC.BPFAMOLLEV.checkStageAndStatus(formContext);
        });
    },

    checkStageAndStatus: function (formContext) {
        debugger;

        var stage = formContext.data.process.getActiveStage();
        if (!stage) return;

        if (stage.getName() === "Employer Verification") {
            // Just wait for user decision
        }
    },

    handleDecision: function (executionContext) {
        debugger;

        var formContext = executionContext.getFormContext();

        var decision =
            formContext.getAttribute("bcc_employerverification")?.getValue();

        var REJECT = 755700001;
        var APPROVE = 755700000;

        //  REJECT â†’ open another form
        if (decision === REJECT) {

            setTimeout(function () {
                debugger;
                Xrm.Navigation.openForm({
                    entityName: formContext.data.entity.getEntityName(),
                    entityId: formContext.data.entity.getId(),
                    formId: "ea396a4e-31e6-f011-8406-70a8a52030e9"
                });
            }, 1000);
        }

        //  APPROVE â†’ move next stage
        if (decision === APPROVE) {
            formContext.data.process.moveNext();
        }
    }
};






var BCC = BCC || {};

BCC.JobApp = {

    onEmployeeVerificationChange: function (executionContext) {
        debugger;

        var formContext = executionContext.getFormContext();

        var verification =
            formContext.getAttribute("bcc_employerverification").getValue();

        var APPROVED = 755700000;
        var CLOSE_STATUS = 755700001;

        if (verification === APPROVED) {

            // Status Reason
            formContext.getAttribute("statuscode").setValue(CLOSE_STATUS);

            //  MUST SAVE
            formContext.data.entity.save();
        }
    }
};



var ManpowerQuota = ManpowerQuota || {};

ManpowerQuota.lockFieldsAfterSave = function (executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();

    // Set default value for quota used
    var quotaUsed = formContext.getAttribute("bcc_quotaused");
    if (quotaUsed && quotaUsed.getValue() === null) {
        quotaUsed.setValue(0);
    }

    // Run only after record is saved
    if (formContext.ui.getFormType() !== 1) {

        var fieldsToLock = [
            "bcc_designationasperquota",
            "bcc_quotaapprovedon",
            "bcc_quotapending",
            "bcc_quotaused"
        ];

        // Lock common fields
        fieldsToLock.forEach(function (fieldName) {
            debugger;
            var control = formContext.getControl(fieldName);
            if (control) {
                control.setDisabled(true);
            }
        });

        // Role-based logic for quota approved
        var quotaApprovedCtrl = formContext.getControl("bcc_quotaapproved");
        if (quotaApprovedCtrl) {

            var userRoles = Xrm.Utility.getGlobalContext().userSettings.roles;
            var hasAccessRole = false;

            userRoles.forEach(function (role) {
                debugger;
                var roleName = role.name;
                if (
                    roleName === "Project Director" ||
                    roleName === "System Administrator" ||
                    roleName === "PRO-HR"
                ) {
                    hasAccessRole = true;
                }
            });

            // Enable only for allowed roles
            quotaApprovedCtrl.setDisabled(!hasAccessRole);
        }
    }
};




var Bcc_FormHandler = Bcc_FormHandler || {};

Bcc_FormHandler.forceInformationFormOnCreate = function (executionContext) {
    debugger;
    try {
        var formContext = executionContext.getFormContext();

        // 1 = Create (New record)
        if (formContext.ui.getFormType() !== 1) {
            return;
        }

        var INFORMATION_FORM_ID = "7a6e1bf5-82d9-f011-8543-0022480cfbe4";

        var currentFormId = formContext.ui.formSelector.getCurrentItem().getId();
        currentFormId = currentFormId.replace("{", "").replace("}", "").toLowerCase();

        var targetFormId = INFORMATION_FORM_ID.replace("{", "").replace("}", "").toLowerCase();

        // Prevent infinite navigation loop
        if (currentFormId !== targetFormId) {

            var formItem = formContext.ui.formSelector.items.get(targetFormId);

            if (formItem) {
                formItem.navigate();
            }
        }

    } catch (error) {
        console.log("Form switch error: ", error);
    }
};




var Bcc_FormHandler = Bcc_FormHandler || {};
Bcc_FormHandler.openInformationFormOnNew = function (executionContext) {
    debugger;
    try {
        var formContext = executionContext.getFormContext();

        // 1 = Create (New record)
        if (formContext.ui.getFormType() !== 1) {
            return;
        }

        var INFORMATION_FORM_ID = "033cc592-0d62-421a-861c-c06f1e5c572d";

        var currentFormId = formContext.ui.formSelector.getCurrentItem().getId();
        currentFormId = currentFormId.replace(/[{}]/g, "").toLowerCase();

        var targetFormId = INFORMATION_FORM_ID.replace(/[{}]/g, "").toLowerCase();

        // Prevent infinite navigation loop
        if (currentFormId === targetFormId) {
            return; // Already on the target form
        }

        // Navigate to the Information form
        formContext.ui.formSelector.items.forEach(function (item) {
            debugger;
            var itemId = item.getId().replace(/[{}]/g, "").toLowerCase();
            if (itemId === targetFormId) {
                item.navigate();
            }
        });

    } catch (error) {
        console.log("Form switch error: ", error);
    }
};





var BCC = BCC || {};

BCC.BPFAMOLLLL = {

    onLoad: function (executionContext) {
        debugger;

        var formContext = executionContext.getFormContext();

        if (!formContext.data.process) return;

        formContext.data.process.addOnStageChange(function () {
            debugger;

            var stage = formContext.data.process.getActiveStage();
            if (!stage) return;

            var stageName = stage.getName();

            //  EXACT stage name
            if (stageName === "Insurance MOL Form") {

                var targetFormId = "91db6c01-e2ea-f011-8406-70a8a51d70b3";

                //  Delay is VERY important
                setTimeout(function () {
                    debugger;

                    formContext.ui.formSelector.items.forEach(function (item) {
                        debugger;

                        if (item.getId().toLowerCase() === targetFormId.toLowerCase()) {
                            item.navigate();
                        }

                    });

                }, 800); // do NOT remove this
            }
        });
    }
};








// ========================================
// FIELD VISIBILITY AND MANDATORY CONTROL
// FOR APPLICATION FROM FIELD
// ========================================

function ManageApplicationFromFields(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    if (!formContext) return;

    var applicationFromAttr = formContext.getAttribute("bcc_applicationfrom");
    if (!applicationFromAttr) return;

    var applicationValue = applicationFromAttr.getValue();

    var cancellationPaperField = "bcc_cancellationpaper";

    // 755700000 = Inside Country
    // 755700001 = Outside Country

    if (applicationValue === 755700000) {
        // INSIDE COUNTRY - Show field and make it mandatory
        ShowAndMakeMandatory(formContext, cancellationPaperField);

    } else {
        // OUTSIDE COUNTRY or No Selection - Hide field and remove mandatory
        HideAndRemoveMandatory(formContext, cancellationPaperField);
    }
}


// Helper function to show field and make it mandatory
function ShowAndMakeMandatory(formContext, fieldName) {
    debugger;
    // Show the field
    var control = formContext.getControl(fieldName);
    if (control) {
        control.setVisible(true);
    }

    // Make it mandatory
    var attribute = formContext.getAttribute(fieldName);
    if (attribute) {
        attribute.setRequiredLevel("required");
    }
}


// Helper function to hide field and remove mandatory
function HideAndRemoveMandatory(formContext, fieldName) {
    debugger;
    // Hide the field
    var control = formContext.getControl(fieldName);
    if (control) {
        control.setVisible(false);
    }

    // Remove mandatory requirement
    var attribute = formContext.getAttribute(fieldName);
    if (attribute) {
        attribute.setRequiredLevel("none");
    }
}







// ========================================
// EMPLOYER VERIFICATION FIELD CONTROL
// ========================================

function ManageEmployerVerificationFields(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    if (!formContext) return;

    var employerVerificationAttr = formContext.getAttribute("bcc_employerverification");
    if (!employerVerificationAttr) return;

    var verificationValue = employerVerificationAttr.getValue();

    var rejectRemarkField = "bcc_rejectremark";

    // 755700001 = Reject

    if (verificationValue === 755700001) {
        // REJECT - Show field and make it mandatory
        ShowAndMakeMandatory(formContext, rejectRemarkField);

    } else {
        // Other options - Hide field and remove mandatory
        HideAndRemoveMandatory(formContext, rejectRemarkField);
    }
}


// Helper function to show field and make it mandatory
function ShowAndMakeMandatory(formContext, fieldName) {
    debugger;
    // Show the field
    var control = formContext.getControl(fieldName);
    if (control) {
        control.setVisible(true);
    }

    // Make it mandatory
    var attribute = formContext.getAttribute(fieldName);
    if (attribute) {
        attribute.setRequiredLevel("required");
    }
}


// Helper function to hide field and remove mandatory
function HideAndRemoveMandatory(formContext, fieldName) {
    debugger;
    // Hide the field
    var control = formContext.getControl(fieldName);
    if (control) {
        control.setVisible(false);
    }

    // Remove mandatory requirement
    var attribute = formContext.getAttribute(fieldName);
    if (attribute) {
        attribute.setRequiredLevel("none");
        // Clear the field value when hiding
        attribute.setValue(null);
    }
}









// ========================================
// COPY FILES TO JOB OFFER ON SAVE
// ========================================

// Global flag to prevent multiple executions
var isFileCopyInProgress = false;

function CopyFilesToJobOffer(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    if (!formContext) return;

    // Prevent multiple simultaneous executions
    if (isFileCopyInProgress) {
        console.log("File copy already in progress, skipping...");
        return;
    }

    // Check if it's the correct form using GUID
    var formId = formContext.ui.formSelector.getCurrentItem().getId();
    formId = formId.replace(/[{}]/g, "").toLowerCase();
    if (formId !== "ea396a4e-31e6-f011-8406-70a8a52030e9") {
        return;
    }

    var entityName = formContext.data.entity.getEntityName();

    // Only run on Job Application entity
    if (entityName !== "bcc_jobapplication") return;

    // Check if Employer Verification is set to "Approve" (755700000)
    var employerVerification = formContext.getAttribute("bcc_employerverification");
    if (!employerVerification || employerVerification.getValue() !== 755700000) {
        console.log("Employer verification not approved, skipping file copy");
        return;
    }

    var recordId = formContext.data.entity.getId();
    if (!recordId) return;

    recordId = recordId.replace(/[{}]/g, "");

    // Set flag BEFORE starting process
    isFileCopyInProgress = true;

    Xrm.Utility.showProgressIndicator(
        "Files will be copied to Job Offer. Please wait..."
    );

    // ONLY CHANGE: Poll every 5 seconds instead of waiting 7 seconds once
    // Flow needs time to create Job Offer â€” we keep checking until it appears
    PollForJobOfferThenCopy(recordId, formContext, 0, 12);
}


// ========================================
//  NEW: POLL UNTIL JOB OFFER IS READY
// Replaces the setTimeout + single check
// ========================================

function PollForJobOfferThenCopy(jobApplicationId, formContext, attempt, maxAttempts) {
    debugger;

    if (attempt >= maxAttempts) {
        // Tried for 60 seconds, gave up
        Xrm.Utility.closeProgressIndicator();
        isFileCopyInProgress = false;
        Xrm.Navigation.openAlertDialog({
            title: " Job Offer Not Found",
            text: "No Job Offer record found linked to this Job Application.\n\nPlease ensure Job Offer is created first."
        });
        return;
    }

    var fetchXml = [
        "<fetch top='1'>",
        "  <entity name='bcc_joboffer'>",
        "    <attribute name='bcc_jobofferid' />",
        "    <order attribute='createdon' descending='false' />",
        "    <filter>",
        "      <condition attribute='bcc_jobapplicationlookup' operator='eq' value='" + jobApplicationId + "' />",
        "    </filter>",
        "  </entity>",
        "</fetch>"
    ].join("");

    Xrm.WebApi.retrieveMultipleRecords("bcc_joboffer", "?fetchXml=" + encodeURIComponent(fetchXml)).then(
        function success(result) {
            debugger;
            if (result.entities.length > 0) {
                // Job Offer found â€” hand off to your existing function
                var jobOfferId = result.entities[0].bcc_jobofferid;
                console.log("Job Offer found on attempt " + (attempt + 1) + ": " + jobOfferId);
                Xrm.Utility.showProgressIndicator("Job Offer found! Copying files...");
                // Call your existing function directly with the found ID
                CopyAllFilesToJobOffer(jobApplicationId, jobOfferId, formContext);
            } else {
                //  Flow not done yet â€” wait 5 seconds and try again
                //  Flow not done yet â€” wait 5 seconds and try again
                var nextAttempt = attempt + 1;
                console.log("Job Offer not ready yet, attempt " + nextAttempt + " of " + maxAttempts);
                Xrm.Utility.showProgressIndicator(
                    "Files will be copied to Job Offer. Please wait..."
                );
                setTimeout(function () {
                    debugger;
                    PollForJobOfferThenCopy(jobApplicationId, formContext, nextAttempt, maxAttempts);
                }, 5000);
            }
        },
        function error(error) {
            debugger;
            Xrm.Utility.closeProgressIndicator();
            isFileCopyInProgress = false;
            Xrm.Navigation.openAlertDialog({
                title: " Error Finding Job Offer",
                text: "Could not search Job Offer records.\n\nError: " + error.message
            });
        }
    );
}


// ========================================
// FIND JOB OFFER BY JOB APPLICATION
// ========================================

function FindJobOfferByJobApplication(jobApplicationId, formContext) {
    debugger;

    // Check if at least one file field has data before searching
    var cancellationPaper = formContext.getAttribute("bcc_cancellationpaper");
    var workerForm = formContext.getAttribute("bcc_workerform");
    var passport = formContext.getAttribute("bcc_passport");
    var degreeCertificate = formContext.getAttribute("bcc_degreecertificate");
    var passport2 = formContext.getAttribute("bcc_passport2");
    var otherdocument = formContext.getAttribute("bcc_otherdocument");
    var passport3 = formContext.getAttribute("bcc_passport3");
    var degreeCertificate2 = formContext.getAttribute("bcc_degreecertificate2");
    var mohreletter = formContext.getAttribute("bcc_mohreletter");

    var hasAnyFile = false;
    if ((cancellationPaper && cancellationPaper.getValue()) ||
        (workerForm && workerForm.getValue()) ||
        (passport && passport.getValue()) ||
        (degreeCertificate && degreeCertificate.getValue()) ||
        (passport2 && passport2.getValue()) ||
        (otherdocument && otherdocument.getValue()) ||
        (passport3 && passport3.getValue()) ||
        (mohreletter && mohreletter.getValue()) ||
        (degreeCertificate2 && degreeCertificate2.getValue())) {
        hasAnyFile = true;
    }

    if (!hasAnyFile) {
        Xrm.Utility.closeProgressIndicator();
        isFileCopyInProgress = false;
        console.log("No files to copy");
        return;
    }

    var fetchXml = [
        "<fetch top='1'>",
        "  <entity name='bcc_joboffer'>",
        "    <attribute name='bcc_jobofferid' />",
        "    <order attribute='createdon' descending='false' />",
        "    <filter>",
        "      <condition attribute='bcc_jobapplicationlookup' operator='eq' value='" + jobApplicationId + "' />",
        "    </filter>",
        "  </entity>",
        "</fetch>"
    ].join("");

    Xrm.WebApi.retrieveMultipleRecords("bcc_joboffer", "?fetchXml=" + encodeURIComponent(fetchXml)).then(
        function success(result) {
            debugger;
            try {
                if (result.entities.length > 0) {
                    var jobOfferId = result.entities[0].bcc_jobofferid;
                    console.log("Job Offer found: " + jobOfferId);
                    CopyAllFilesToJobOffer(jobApplicationId, jobOfferId, formContext);
                } else {
                    Xrm.Navigation.openAlertDialog({
                        title: "âš ï¸ Job Offer Not Found",
                        text: "No Job Offer record found linked to this Job Application.\n\nPlease ensure Job Offer is created first."
                    });
                }
            } catch (error) {
                console.error("Unexpected error:", error);
                Xrm.Navigation.openAlertDialog({
                    title: " Unexpected Error",
                    text: "Something went wrong.\n\nError: " + error.message
                });
            } finally {
                Xrm.Utility.closeProgressIndicator();
                isFileCopyInProgress = false;
            }
        },
        function error(error) {
            debugger;
            Xrm.Utility.closeProgressIndicator();
            isFileCopyInProgress = false;
            Xrm.Navigation.openAlertDialog({
                title: " Error Finding Job Offer",
                text: "Could not search Job Offer records.\n\nError: " + error.message
            });
        }
    );
}


// ========================================
//  RENAMED from inner block to named function
// Called by both PollForJobOfferThenCopy and FindJobOfferByJobApplication
// ========================================

function CopyAllFilesToJobOffer(jobApplicationId, jobOfferId, formContext) {
    debugger;

    var fieldMappings = [
        { source: "bcc_cancellationpaper", target: "bcc_cancellationform" },
        { source: "bcc_workerform", target: "bcc_workerform" },
        { source: "bcc_passport", target: "bcc_passport" },
        { source: "bcc_degreecertificate", target: "bcc_degreecertificate" },
        { source: "bcc_passport2", target: "bcc_passport2jo" },
        { source: "bcc_otherdocument", target: "bcc_otherdocument" },
        { source: "bcc_passport3", target: "bcc_passport3" },
        { source: "bcc_mohreletter", target: "bcc_mohreletter" },
        { source: "bcc_degreecertificate2", target: "bcc_degreecertificate2" }
    ];

    var copyPromises = [];

    fieldMappings.forEach(function (mapping) {
        debugger;
        var sourceAttr = formContext.getAttribute(mapping.source);
        if (sourceAttr && sourceAttr.getValue()) {
            try {
                var promise = CopyFileField(
                    "bcc_jobapplication",
                    jobApplicationId,
                    mapping.source,
                    "bcc_joboffer",
                    jobOfferId,
                    mapping.target,
                    formContext
                );
                if (promise) copyPromises.push(promise);
            } catch (innerError) {
                console.error("Error copying field:", mapping.source, innerError);
            }
        }
    });

    if (copyPromises.length === 0) {
        console.log("No files available to copy");
        Xrm.Utility.closeProgressIndicator();
        isFileCopyInProgress = false;
        return;
    }

    Promise.all(copyPromises)
        .then(function () {
            debugger;
            Xrm.Navigation.openAlertDialog({
                title: " Files Copied Successfully",
                text: "All files have been copied to Job Offer."
            });
        })
        .catch(function (error) {
            debugger;
            Xrm.Navigation.openAlertDialog({
                title: " Copy Failed",
                text: "Some files could not be copied.\n\nError: " + error
            });
        })
        .finally(function () {
            debugger;
            Xrm.Utility.closeProgressIndicator();
            isFileCopyInProgress = false;
        });
}


// ========================================
// COPY SINGLE FILE FIELD
// ========================================

function CopyFileField(
    sourceEntityName,
    sourceRecordId,
    sourceFieldName,
    targetEntityName,
    targetRecordId,
    targetFieldName,
    formContext
) {
    return new Promise(function (resolve, reject) {
        debugger;

        console.log("Starting copy: " + sourceFieldName + " -> " + targetFieldName);

        Xrm.Utility.getEntityMetadata(sourceEntityName).then(
            function (sourceEntityData) {
                debugger;

                var sourceEntitySetName = sourceEntityData.EntitySetName;

                var downloadUrl =
                    Xrm.Utility.getGlobalContext().getClientUrl() +
                    "/api/data/v9.2/" + sourceEntitySetName +
                    "(" + sourceRecordId + ")/" + sourceFieldName + "/$value";

                var downloadReq = new XMLHttpRequest();
                downloadReq.open("GET", downloadUrl, true);
                downloadReq.setRequestHeader("OData-MaxVersion", "4.0");
                downloadReq.setRequestHeader("OData-Version", "4.0");
                downloadReq.setRequestHeader("Accept", "application/octet-stream");
                downloadReq.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
                downloadReq.responseType = "blob";

                downloadReq.onload = function () {
                    debugger;
                    if (downloadReq.status === 200) {

                        var blob = downloadReq.response;
                        var contentDisposition = downloadReq.getResponseHeader("Content-Disposition");
                        var contentType = downloadReq.getResponseHeader("Content-Type") || blob.type || "image/jpeg";
                        var fileName = "file";

                        if (contentDisposition) {
                            var fileNameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
                            if (fileNameMatch && fileNameMatch[1]) {
                                fileName = fileNameMatch[1].replace(/['"]/g, '');
                                fileName = decodeURIComponent(fileName);
                            }
                        }

                        console.log("Downloaded: " + fileName + " (" + blob.size + " bytes) - Type: " + contentType);

                        // Resize image ONLY if it's the bcc_image field
                        if (sourceFieldName === "bcc_image") {
                            var reader = new FileReader();
                            reader.onload = function (e) {
                                debugger;
                                var img = new Image();
                                img.onload = function () {
                                    debugger;
                                    var canvas = document.createElement("canvas");
                                    var targetSize = 600;
                                    canvas.width = targetSize;
                                    canvas.height = targetSize;
                                    var ctx = canvas.getContext("2d");
                                    ctx.imageSmoothingEnabled = true;
                                    ctx.imageSmoothingQuality = "high";
                                    ctx.drawImage(img, 0, 0, targetSize, targetSize);
                                    canvas.toBlob(function (resizedBlob) {
                                        debugger;
                                        console.log("Image resized to 600x600 with high quality");
                                        UploadFileToTarget(
                                            targetEntityName, targetRecordId, targetFieldName,
                                            fileName, resizedBlob, contentType,
                                            sourceFieldName, resolve, reject
                                        );
                                    }, contentType, 1.0);
                                };
                                img.src = e.target.result;
                            };
                            reader.readAsDataURL(blob);
                        } else {
                            UploadFileToTarget(
                                targetEntityName, targetRecordId, targetFieldName,
                                fileName, blob, contentType,
                                sourceFieldName, resolve, reject
                            );
                        }

                    } else if (downloadReq.status === 404) {
                        console.log("No file found in field: " + sourceFieldName);
                        resolve("No file");
                    } else {
                        console.error("Download failed for " + sourceFieldName + ": " + downloadReq.status);
                        reject("Download failed: " + downloadReq.status);
                    }
                };

                downloadReq.onerror = function () {
                    debugger;
                    console.error("Network error downloading from: " + sourceFieldName);
                    reject("Download network error");
                };

                downloadReq.send();
            },
            function (error) {
                debugger;
                console.error("Metadata error (source): " + error.message);
                reject("Metadata error: " + error.message);
            }
        );
    });
}


// ========================================
// UPLOAD FILE TO TARGET ENTITY
// ========================================

function UploadFileToTarget(
    targetEntityName,
    targetRecordId,
    targetFieldName,
    fileName,
    fileData,
    contentType,
    sourceFieldName,
    resolve,
    reject
) {
    Xrm.Utility.getEntityMetadata(targetEntityName).then(
        function (targetEntityData) {
            debugger;

            var targetEntitySetName = targetEntityData.EntitySetName;

            var uploadUrl =
                Xrm.Utility.getGlobalContext().getClientUrl() +
                "/api/data/v9.2/" + targetEntitySetName +
                "(" + targetRecordId + ")/" + targetFieldName;

            var uploadReq = new XMLHttpRequest();
            uploadReq.open("PATCH", uploadUrl, true);
            uploadReq.setRequestHeader("OData-MaxVersion", "4.0");
            uploadReq.setRequestHeader("OData-Version", "4.0");
            uploadReq.setRequestHeader("Accept", "application/json");
            uploadReq.setRequestHeader("Content-Type", contentType);
            uploadReq.setRequestHeader("x-ms-file-name", fileName);
            uploadReq.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");

            uploadReq.onload = function () {
                debugger;
                if (uploadReq.status === 204 || uploadReq.status === 200) {
                    console.log(" Uploaded: " + fileName + " to " + targetFieldName + " (High quality preserved)");
                    resolve("Success: " + fileName);
                } else {
                    console.error("Upload failed for " + fileName + ": " + uploadReq.status);
                    console.error("Response: " + uploadReq.responseText);
                    reject("Upload failed: " + uploadReq.status);
                }
            };

            uploadReq.onerror = function () {
                debugger;
                console.error("Network error uploading to: " + targetFieldName);
                reject("Upload network error");
            };

            uploadReq.send(fileData);
        },
        function (error) {
            debugger;
            console.error("Metadata error (target): " + error.message);
            reject("Metadata error: " + error.message);
        }
    );
}








function BPFAMOLLLLEM(executionContext) {
    debugger;

    var formContext = executionContext.getFormContext();

    if (!formContext.data.process) return;

    formContext.data.process.addOnStageChange(function () {
        debugger;

        var stage = formContext.data.process.getActiveStage();
        if (!stage) return;

        var stageName = stage.getName();

        //  EXACT stage name
        if (stageName === "Employer Verification") {

            var targetFormId = "af9d43d6-76ec-f011-8406-70a8a52260b1";

            //  Delay is VERY important
            setTimeout(function () {
                debugger;

                formContext.ui.formSelector.items.forEach(function (item) {
                    debugger;

                    if (item.getId().toLowerCase() === targetFormId.toLowerCase()) {
                        item.navigate();
                    }

                });

            }, 800); // <-- do NOT remove this
        }
    });
}




function PreApprovalWorkPermit(executionContext) {
    debugger;

    var formContext = executionContext.getFormContext();

    //  Get current form ID
    var currentFormId = formContext.ui.formSelector.getCurrentItem().getId();

    var targetFormId = "778573dd-2ce6-f011-8406-70a8a51e50d4";

    //  Check if current form matches
    if (currentFormId.toLowerCase() === targetFormId.toLowerCase()) {

        var statusField = formContext.getAttribute("statuscode");

        if (statusField) {
            statusField.setValue(100000001);
            statusField.setSubmitMode("always");
        }
    }
}





function InsuranceMOLForm(executionContext) {
    debugger;

    var formContext = executionContext.getFormContext();
    if (!formContext.data.process) return;

    formContext.data.process.addOnStageChange(function () {
        debugger;

        var stage = formContext.data.process.getActiveStage();
        if (!stage) return;

        var stageName = stage.getName(); // â† preserved variable line

        if (stageName === "Insurance MOL Form") {

            var targetFormId = "91db6c01-e2ea-f011-8406-70a8a51d70b3";

            setTimeout(function () {
                debugger;

                formContext.ui.formSelector.items.forEach(function (item) {
                    debugger;
                    if (item.getId().toLowerCase() === targetFormId.toLowerCase()) {
                        item.navigate();
                    }
                });

            }, 800);
        }
    });
}






function Labourfee(executionContext) {
    debugger;

    var formContext = executionContext.getFormContext();
    if (!formContext.data.process) return;

    formContext.data.process.addOnStageChange(function () {
        debugger;

        var stage = formContext.data.process.getActiveStage();
        if (!stage) return;

        var stageName = stage.getName(); // â† preserved variable line

        if (stageName === "Labour Fee") {

            var targetFormId = "2475379f-5df0-f011-8406-70a8a52030e9";

            setTimeout(function () {
                debugger;

                formContext.ui.formSelector.items.forEach(function (item) {
                    debugger;
                    if (item.getId().toLowerCase() === targetFormId.toLowerCase()) {
                        item.navigate();
                    }
                });

            }, 800);
        }
    });
}









function VisaFeecs(executionContext) {
    debugger;

    var formContext = executionContext.getFormContext();
    if (!formContext.data.process) return;

    // Check if bcc_applyvisa field value is 755700000 (Inside Country)
    var applyVisaField = formContext.getAttribute("bcc_applyvisa");
    if (!applyVisaField) return;

    var applyVisaValue = applyVisaField.getValue();
    if (applyVisaValue !== 755700000) return; // Only run if "Inside Country" is selected

    formContext.data.process.addOnStageChange(function () {
        debugger;

        var stage = formContext.data.process.getActiveStage();
        if (!stage) return;

        var stageName = stage.getName();

        if (stageName === "Change Status") {

            var targetFormId = "eb0903e4-5ef0-f011-8406-70a8a52030e9";

            setTimeout(function () {
                debugger;

                formContext.ui.formSelector.items.forEach(function (item) {
                    debugger;
                    if (item.getId().toLowerCase() === targetFormId.toLowerCase()) {
                        item.navigate();
                    }
                });

            }, 800);
        }
    });
}




function EIDfee(executionContext) {
    debugger;

    var formContext = executionContext.getFormContext();
    if (!formContext.data.process) return;

    formContext.data.process.addOnStageChange(function () {
        debugger;

        var stage = formContext.data.process.getActiveStage();
        if (!stage) return;

        var stageName = stage.getName(); // â† preserved variable line

        if (stageName === "EID Fee") {

            var targetFormId = "080ee06f-5ff0-f011-8406-0022480de9a8";

            setTimeout(function () {
                debugger;

                formContext.ui.formSelector.items.forEach(function (item) {
                    debugger;
                    if (item.getId().toLowerCase() === targetFormId.toLowerCase()) {
                        item.navigate();
                    }
                });

            }, 800);
        }
    });
}





function ILOEfee(executionContext) {
    debugger;

    var formContext = executionContext.getFormContext();
    if (!formContext.data.process) return;

    formContext.data.process.addOnStageChange(function () {
        debugger;

        var stage = formContext.data.process.getActiveStage();
        if (!stage) return;

        var stageName = stage.getName(); // â† preserved variable line

        if (stageName === "ILOE Fee") {

            var targetFormId = "80cc2fc5-5ff0-f011-8406-0022480de9a8";

            setTimeout(function () {
                debugger;

                formContext.ui.formSelector.items.forEach(function (item) {
                    debugger;
                    if (item.getId().toLowerCase() === targetFormId.toLowerCase()) {
                        item.navigate();
                    }
                });

            }, 800);
        }
    });
}






function Tawjeefee(executionContext) {
    debugger;

    var formContext = executionContext.getFormContext();
    if (!formContext.data.process) return;

    formContext.data.process.addOnStageChange(function () {
        debugger;

        var stage = formContext.data.process.getActiveStage();
        if (!stage) return;

        var stageName = stage.getName(); // â† preserved variable line

        if (stageName === "Tawjee Fee") {

            var targetFormId = "874e5278-60f0-f011-8406-70a8a52260b1";

            setTimeout(function () {
                debugger;

                formContext.ui.formSelector.items.forEach(function (item) {
                    debugger;
                    if (item.getId().toLowerCase() === targetFormId.toLowerCase()) {
                        item.navigate();
                    }
                });

            }, 800);
        }
    });
}







function Residency(executionContext) {
    debugger;

    var formContext = executionContext.getFormContext();
    if (!formContext.data.process) return;

    formContext.data.process.addOnStageChange(function () {
        debugger;

        var stage = formContext.data.process.getActiveStage();
        if (!stage) return;

        var stageName = stage.getName(); // â† preserved variable line

        if (stageName === "Residency") {

            var targetFormId = "76375402-61f0-f011-8406-70a8a522b0a0";

            setTimeout(function () {
                debugger;

                formContext.ui.formSelector.items.forEach(function (item) {
                    debugger;
                    if (item.getId().toLowerCase() === targetFormId.toLowerCase()) {
                        item.navigate();
                    }
                });

            }, 800);
        }
    });
}








function Medicalfee(executionContext) {
    debugger;

    var formContext = executionContext.getFormContext();
    if (!formContext.data.process) return;

    formContext.data.process.addOnStageChange(function () {
        debugger;

        var stage = formContext.data.process.getActiveStage();
        if (!stage) return;

        var stageName = stage.getName(); // â† preserved variable line

        if (stageName === "Medical Fee") {

            var targetFormId = "1eb03537-5ff0-f011-8406-70a8a522b0a0";

            setTimeout(function () {
                debugger;

                formContext.ui.formSelector.items.forEach(function (item) {
                    debugger;
                    if (item.getId().toLowerCase() === targetFormId.toLowerCase()) {
                        item.navigate();
                    }
                });

            }, 800);
        }
    });
}





function VisaFeee(executionContext) {
    debugger;

    var formContext = executionContext.getFormContext();
    if (!formContext.data.process) return;

    formContext.data.process.addOnStageChange(function () {
        debugger;

        var stage = formContext.data.process.getActiveStage();
        if (!stage) return;

        var stageName = stage.getName(); // â† preserved variable line

        if (stageName === "Visa Fee") {

            var targetFormId = "5b8da095-5ef0-f011-8406-70a8a52260b1";

            setTimeout(function () {
                debugger;

                formContext.ui.formSelector.items.forEach(function (item) {
                    debugger;
                    if (item.getId().toLowerCase() === targetFormId.toLowerCase()) {
                        item.navigate();
                    }
                });

            }, 800);
        }
    });
}




function SetStageNameToStatusReason(executionContext) {
    debugger;

    var formContext = executionContext.getFormContext();
    if (!formContext || !formContext.data || !formContext.data.process) return;

    function setStatus() {
        debugger;

        var stage = formContext.data.process.getActiveStage();
        if (!stage) return;

        var stageName = stage.getName();

        var statusAttr = formContext.getAttribute("statuscode");
        if (!statusAttr) return;

        var options = statusAttr.getOptions();
        var targetValue = null;

        for (var i = 0; i < options.length; i++) {
            if (options[i].text === stageName) {
                targetValue = options[i].value;
                break;
            }
        }

        if (targetValue === null) return;

        //  Only set (NO SAVE)
        statusAttr.setValue(targetValue);
        statusAttr.setSubmitMode("always");
    }

    // Delay for proper stage detection
    setTimeout(setStatus, 800);
}


function VisaFeemf(executionContext) {
    debugger;

    var formContext = executionContext.getFormContext();
    if (!formContext.data.process) return;

    // Check if bcc_applyvisa field value is 755700001 (Outside Country)
    var applyVisaField = formContext.getAttribute("bcc_applyvisa");
    if (!applyVisaField) return;

    var applyVisaValue = applyVisaField.getValue();
    if (applyVisaValue !== 755700001) return; // Only run if "Outside Country" is selected

    // Remove existing handler to avoid duplicates
    formContext.data.process.removeOnStageChange(stageChangeHandler);

    // Add stage change handler
    formContext.data.process.addOnStageChange(stageChangeHandler);

    function stageChangeHandler() {
        debugger;
        var stage = formContext.data.process.getActiveStage();
        if (!stage) return;

        var stageName = stage.getName();

        if (stageName === "Medical Fee") {
            var targetFormId = "1eb03537-5ff0-f011-8406-70a8a522b0a0";

            setTimeout(function () {
                debugger;
                formContext.ui.formSelector.items.forEach(function (item) {
                    debugger;
                    if (item.getId().toLowerCase() === targetFormId.toLowerCase()) {
                        item.navigate();
                    }
                });
            }, 800);
        }
    }
}
function setSignedFieldsRequired(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();

    var employeeContract = formContext.getAttribute("bcc_employeecontract");
    var jobOffer = formContext.getAttribute("bcc_joboffer");

    var signedJobOffer = formContext.getAttribute("bcc_signedjoboffer");
    var signedContract = formContext.getAttribute("bcc_signcontract");

    if (!employeeContract || !jobOffer || !signedJobOffer || !signedContract) {
        return;
    }

    var hasEmployeeContract = employeeContract.getValue() !== null;
    var hasJobOffer = jobOffer.getValue() !== null;

    if (hasEmployeeContract || hasJobOffer) {
        signedJobOffer.setRequiredLevel("required");
        signedContract.setRequiredLevel("required");
    } else {
        signedJobOffer.setRequiredLevel("none");
        signedContract.setRequiredLevel("none");
    }
}






function setStatusOnBPFComplete(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();

    // Check if we're on the correct form
    var currentFormId = formContext.ui.formSelector.getCurrentItem().getId().toLowerCase().replace(/[{}]/g, "");
    var targetFormId = "76375402-61f0-f011-8406-70a8a522b0a0";

    if (currentFormId !== targetFormId) {
        return;
    }

    if (!formContext.data.process) {
        return;
    }

    // Listen for stage changes (fires when Finish button is clicked)
    formContext.data.process.addOnStageChange(function () {
        debugger;
        // Wait briefly for BPF stage change to complete
        setTimeout(function () {
            debugger;
            checkIfBPFCompleted(formContext);
        }, 2000);
    });
}

function checkIfBPFCompleted(formContext) {
    debugger;
    try {
        var activeStageAttr = formContext.getAttribute("activestage");

        if (!activeStageAttr) {
            console.error("activestage attribute not found");
            return;
        }

        var activeStageValue = activeStageAttr.getValue();

        // When BPF is finished, activestage becomes null
        if (activeStageValue === null || activeStageValue === "") {
            var statusAttr = formContext.getAttribute("statuscode");
            var stateAttr = formContext.getAttribute("statecode");

            if (!statusAttr || !stateAttr) {
                console.error("statuscode or statecode attribute not found");
                return;
            }

            var currentStatus = statusAttr.getValue();

            // Only update if not already set
            if (currentStatus !== 100000003) {
                // Step 1: Set statecode to Inactive
                stateAttr.setValue(1);

                formContext.data.save().then(function () {
                    debugger;
                    // Step 2: After save, set statuscode
                    statusAttr.setValue(100000003);
                    formContext.data.save().then(
                        function () {
                            debugger;
                            console.log("âœ“âœ“ SUCCESS! Status Reason is now 100000003");
                        },
                        function (error) {
                            debugger;
                            console.error("âœ— ERROR while saving statuscode: " + error.message);
                        }
                    );
                }, function (error) {
                    debugger;
                    console.error("âœ— ERROR while saving statecode: " + error.message);
                });

            } else {
                console.log("Status already set to 100000003");
            }
        } else {
            console.log("BPF not finished yet. Active stage still exists.");
        }

    } catch (e) {
        console.error("âœ— ERROR: " + e.message);
    }
}






// ===================================
// REGISTER ON FORM ONSAVE EVENT
// ===================================
function checkBPFOnSave(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();

    // Check if we're on the correct form
    try {
        var currentFormId = formContext.ui.formSelector.getCurrentItem().getId().toLowerCase().replace(/[{}]/g, "");
        var targetFormId = "76375402-61f0-f011-8406-70a8a522b0a0";

        if (currentFormId !== targetFormId) {
            return;
        }
    } catch (e) {
        // Continue if form check fails
    }

    if (!formContext.data.process) {
        return;
    }

    try {
        // Get the processid field (hidden field that becomes null when BPF finishes)
        var processIdAttr = formContext.getAttribute("processid");

        if (!processIdAttr) {
            checkByStageCompletion(formContext);
            return;
        }

        var processIdValue = processIdAttr.getValue();

        // If processid is null, BPF is finished
        if (processIdValue === null || processIdValue === "") {
            updateStatusToHiredAndDeactivate(formContext);
        } else {
            // Also check if we're on last stage with all steps complete
            checkByStageCompletion(formContext);
        }

    } catch (e) {
        alert("Error: " + e.message);
    }
}

function checkByStageCompletion(formContext) {
    debugger;
    try {
        var activeStage = formContext.data.process.getActiveStage();

        if (!activeStage) {
            updateStatusToHiredAndDeactivate(formContext);
            return;
        }

        var stageName = activeStage.getName();

        // Check if we're on Residency (last stage)
        if (stageName === "Residency") {
            // Check if all required fields are filled
            var steps = activeStage.getSteps();
            var allComplete = true;

            for (var i = 0; i < steps.getLength(); i++) {
                var step = steps.get(i);
                if (step.isRequired()) {
                    var stepAttr = step.getAttribute();
                    if (!stepAttr.getValue()) {
                        allComplete = false;
                        break;
                    }
                }
            }

            if (allComplete) {
                updateStatusToHiredAndDeactivate(formContext);
            }
        }

    } catch (e) {
        // Error in stage check
    }
}

function updateStatusToHiredAndDeactivate(formContext) {
    debugger;
    try {
        var statusAttr = formContext.getAttribute("statuscode");
        var stateAttr = formContext.getAttribute("statecode");
        if (!statusAttr || !stateAttr) {
            return;
        }

        var currentStatus = statusAttr.getValue();

        if (currentStatus !== 100000003) {
            // Set status to Hired
            stateAttr.setValue(1); // Inactive
            statusAttr.setValue(100000003);

            // alert("Status set to Hired. Record will be deactivated after save.");
        }

    } catch (e) {
        alert("Error updating status: " + e.message);
    }
}

// Helper function to deactivate record using Web API
function deactivateRecord(formContext) {
    debugger;
    try {
        var entityName = formContext.data.entity.getEntityName();
        var recordId = formContext.data.entity.getId().replace(/[{}]/g, "");

        // Use SetState request to properly deactivate the record
        var parameters = {
            State: { Value: 1 }, // Inactive
            Status: { Value: 2 }  // Inactive status (default inactive status reason)
        };

        var req = {
            entity: {
                id: recordId,
                entityType: entityName
            },
            State: parameters.State,
            Status: parameters.Status,

            getMetadata: function () {
                debugger;
                return {
                    boundParameter: "entity",
                    parameterTypes: {
                        "entity": {
                            typeName: "mscrm." + entityName,
                            structuralProperty: 5
                        },
                        "State": {
                            typeName: "mscrm.bcc_joboffer_bcc_joboffer_statecode",
                            structuralProperty: 1
                        },
                        "Status": {
                            typeName: "mscrm.bcc_jobofferstatecode",
                            structuralProperty: 1
                        }
                    },
                    operationType: 0,
                    operationName: "SetState"
                };
            }
        };

        Xrm.WebApi.online.execute(req).then(
            function () {
                debugger;
                alert("Record deactivated successfully!");
                formContext.data.refresh(true);
            },
            function (error) {
                debugger;
                alert("Error deactivating record: " + error.message);
            }
        );

    } catch (e) {
        alert("Error in deactivate function: " + e.message);
    }
}





async function createContact_OnSave(executionContext) {
    debugger;
    try {
        const formContext = executionContext.getFormContext();
        const contactAttr = formContext.getAttribute("bcc_contact");

        if (contactAttr && contactAttr.getValue()) {
            return; // Contact already exists
        }

        const getValue = (field) => {
            const attr = formContext.getAttribute(field);
            return attr ? attr.getValue() : null;
        };

        const firstName = getValue("new_givennames");
        const lastName = getValue("bcc_lastname");

        if (!firstName || !lastName) {
            return; // Required fields missing
        }

        //  FIX: Event args ko pehle get karo, baad mein prevent karo
        // const eventArgs = executionContext.getEventArgs();

        var contact = {
            firstname: firstName,
            lastname: lastName,
            bcc_designationaspervisa: getValue("bcc_designationaspervisa"),
            bcc_dateofbirth: getValue("bcc_dateofbirth"),
            bcc_labourcardexpiry: getValue("bcc_labourcardexpiry"),
            bcc_placeofbirth: getValue("bcc_placeofbirth"),
            bcc_molnumber: getValue("bcc_molnumber"),
            bcc_uidno: getValue("bcc_uidno"),
            bcc_workmanscomp: getValue("bcc_workmanscomp"),
            bcc_probationcompletiondate: getValue("bcc_probationcompletiondate"),
            bcc_emiratesid: getValue("bcc_emiratesid"),
            bcc_ppexpirydate: getValue("bcc_ppexpirydate"),
            bcc_visano: getValue("bcc_visano"),
            bcc_designationincompany: getValue("bcc_designationincompany"),
            bcc_visaexpirydate: getValue("bcc_visaexpirydate"),
            bcc_labourcardno: getValue("bcc_labourcardno"),
            bcc_passportno: getValue("bcc_passportno"),
            bcc_nationality_: getValue("bcc_nationality_"),
            bcc_placeofissue: getValue("bcc_placeofissue"),
            bcc_medicalinsurance: getValue("bcc_medicalinsurance"),
            bcc_joiningdate: getValue("bcc_joiningdate"),
            bcc_givennames: getValue("bcc_name")
        };

        // Remove NULL, undefined, aur arrays
        Object.keys(contact).forEach(key => {
            const value = contact[key];
            if (value === null || value === undefined || Array.isArray(value)) {
                delete contact[key];
            }
        });

        console.log(" Creating contact...");

        //  IMPORTANT: Prevent save ONLY if we're creating contact
        // eventArgs.preventDefault();

        const result = await Xrm.WebApi.createRecord("contact", contact);

        console.log(" Contact created:", result.id);

        contactAttr.setValue([{
            id: result.id,
            name: `${firstName} ${lastName}`,
            entityType: "contact"
        }]);

        contactAttr.setSubmitMode("always");

        //  FIX: Save with saveOptions to suppress errors


    } catch (e) {
        console.error(" Error:", e);
        Xrm.Navigation.openAlertDialog({
            text: " ERROR!\n\n" + e.message
        });
    }
}






var YEAR_MIN = 1950;
var YEAR_MAX = 2050;

// OnChange handler for all three number fields
function validateYearNumberOnChange(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var attribute = executionContext.getEventSource();

    if (!attribute) return;

    var fieldName = attribute.getName();
    var allowedFields = [
        "bcc_graduationyear",
        "bcc_degreestartdate",
        "bcc_degreeenddate"
    ];

    if (allowedFields.indexOf(fieldName) === -1) return;

    var value = attribute.getValue();
    if (value === null) return;

    if (value < YEAR_MIN || value > YEAR_MAX) {
        Xrm.Navigation.openAlertDialog({
            text: "Only years between 1950 and 2050 are allowed."
        });
        attribute.setValue(null);   //  value vanishes
        attribute.setSubmitMode("always");
    }
}

// OnSave safety check (blocks save)
function validateYearNumberOnSave(executionContext) {
    debugger;
    var eventArgs = executionContext.getEventArgs();
    var formContext = executionContext.getFormContext();

    var fields = [
        "bcc_graduationyear",
        "bcc_degreestartdate",
        "bcc_degreeenddate"
    ];

    for (var i = 0; i < fields.length; i++) {
        var attr = formContext.getAttribute(fields[i]);
        if (!attr) continue;

        var value = attr.getValue();
        if (value === null) continue;

        if (value < YEAR_MIN || value > YEAR_MAX) {
            Xrm.Navigation.openAlertDialog({
                text: "Record cannot be saved. Years must be between 1950 and 2050."
            });
            eventArgs.preventDefault(); //  block save
            return;
        }
    }
}






function toggleEducationFieldsMandatory(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();

    var triggerField = "bcc_applicationfrom_";
    var triggerValue = 755700001;

    var targetFields = [
        "bcc_universityname",
        "bcc_studymajors",
        "bcc_degreestartdate",
        "bcc_graduationyear",
        "bcc_degreetype",
        "bcc_faculty",
        "bcc_degreeenddate"
    ];

    var applicationFromAttr = formContext.getAttribute(triggerField);
    if (!applicationFromAttr) return;

    var selectedValue = applicationFromAttr.getValue();
    var makeRequired = (selectedValue === triggerValue);

    for (var i = 0; i < targetFields.length; i++) {
        var attr = formContext.getAttribute(targetFields[i]);
        if (attr) {
            attr.setRequiredLevel(makeRequired ? "required" : "none");
        }
    }
}










function filterUnitLookup(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var unitControl = formContext.getControl("bcc_unit");

    if (unitControl) {
        unitControl.addPreSearch(function () {
            debugger;

            var filter = "<filter type='and'>" +
                "<condition attribute='bcc_unitstatus' operator='eq' value='755700000' />" +
                "</filter>";

            unitControl.addCustomFilter(filter);
        });
    }
}

function filterUnitLookup(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var unitControl = formContext.getControl("bcc_unit");

    if (unitControl) {
        unitControl.addPreSearch(function () {
            debugger;

            var filter = "<filter type='and'>" +
                "<condition attribute='bcc_unitstatus' operator='eq' value='755700000' />" +
                "</filter>";

            unitControl.addCustomFilter(filter);
        });
    }
}


function setUnitOccupiedOnSave(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();

    var unitValue = formContext.getAttribute("bcc_unit").getValue();

    if (!unitValue) return;

    var unitId = unitValue[0].id.replace("{", "").replace("}", "");

    var data = {
        "bcc_unitstatus": 755700001 // Occupied
    };

    Xrm.WebApi.updateRecord("bcc_unit", unitId, data).then(
        function success() {
            debugger;
            console.log("Unit marked as Occupied");
        },
        function error(err) {
            debugger;
            console.log(err.message);
        }
    );
}






function copyFileToImage(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var entityName = formContext.data.entity.getEntityName();
    var recordId = formContext.data.entity.getId();

    if (!recordId) {
        console.log("Please save the record first");
        return;
    }

    recordId = recordId.replace(/[{}]/g, "");

    var query = "?$select=bcc_jafile";

    Xrm.WebApi.retrieveRecord(entityName, recordId, query).then(
        function (result) {
            debugger;
            if (result.bcc_jafile) {
                var data = {};
                data.bcc_image = result.bcc_jafile;

                Xrm.WebApi.updateRecord(entityName, recordId, data).then(
                    function () {
                        debugger;
                        console.log("Success");
                        formContext.data.refresh(false);
                    },
                    function (error) {
                        debugger;
                        console.log("Update error: " + error.message);
                    }
                );
            } else {
                console.log("No file found");
            }
        },
        function (error) {
            debugger;
            console.log("Retrieve error: " + error.message);
        }
    );
}







function AssetType_OnChange(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();

    var assetType = formContext.getAttribute("bcc_assettype");
    if (!assetType) return;

    var assetTypeValue = assetType.getValue();


    var mobileFields = [
        "new_mobile",
        "new_mobileplan",
        "new_startdate",
        "new_enddate"
    ];


    var assetInfoFields = [
        "bcc_software",
        "bcc_modelno",
        "bcc_serialnumber",
        "new_configuration"
    ];

    function showFields(fields) {
        debugger;
        fields.forEach(function (field) {
            debugger;
            var ctrl = formContext.getControl(field);
            if (ctrl) ctrl.setVisible(true);
        });
    }

    function hideFields(fields) {
        debugger;
        fields.forEach(function (field) {
            debugger;
            var ctrl = formContext.getControl(field);
            if (ctrl) ctrl.setVisible(false);
        });
    }

    function showAllFields() {
        debugger;
        formContext.ui.controls.forEach(function (control) {
            debugger;
            if (control && control.setVisible) {
                control.setVisible(true);
            }
        });
    }



    if (assetTypeValue === 755700002) {


        showFields(["new_mobile", "new_mobileplan"]);


        hideFields(assetInfoFields);
        hideFields(["new_startdate", "new_enddate"]);

    }
    else if (assetTypeValue === 755700000) {


        showFields(assetInfoFields);


        hideFields(mobileFields);

    }
    else {


        showAllFields();
    }
}












function onFileChangeforsigncontract(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();

    // Get file field value
    var fileValue = formContext.getAttribute("bcc_signcontract").getValue();

    // Get date field
    var dateField = formContext.getAttribute("bcc_signcontractdocdate");

    // If file uploaded, set current date. If deleted, set null
    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}






// For bcc_passport
function onFileChangeForPassport(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_passport").getValue();
    var dateField = formContext.getAttribute("bcc_passportjauploaddste");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}

// For bcc_passport2
function onFileChangeForPassport2(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_passport2").getValue();
    var dateField = formContext.getAttribute("bcc_passport2jauploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}

// For bcc_passport3
function onFileChangeForPassport3(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_passport3").getValue();
    var dateField = formContext.getAttribute("bcc_passport3jauploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}

// For bcc_jafile
function onFileChangeForJaFile(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_jafile").getValue();
    var dateField = formContext.getAttribute("bcc_photojauploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}
// Medical Certificate
function onMedicalFileChange(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_medicalcertificate").getValue();
    var dateField = formContext.getAttribute("bcc_medicalcertificateuploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}
//For medical result
function onMedicalResultChange(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_medicalresult").getValue();
    var dateField = formContext.getAttribute("bcc_medicalresultuploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}
// For bcc_degreecertificate
function onFileChangeForDegreeCertificate(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_degreecertificate").getValue();
    var dateField = formContext.getAttribute("bcc_degreecertificatejauploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}

// For bcc_degreecertificate2
function onFileChangeForDegreeCertificate2(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_degreecertificate2").getValue();
    var dateField = formContext.getAttribute("bcc_degreecertificate2jauploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}

// For bcc_workerform
function onFileChangeForWorkerForm(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_workerform").getValue();
    var dateField = formContext.getAttribute("bcc_workerformjauploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}

// For bcc_otherdocument
function onFileChangeForOtherDocument(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_otherdocument").getValue();
    var dateField = formContext.getAttribute("bcc_otherdocumentjauploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}







































// For bcc_passport
function onFileChangeForPassport1(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_passport").getValue();
    var dateField = formContext.getAttribute("bcc_passportuploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}

// For bcc_passport2jo
function onFileChangeForPassport2jo1(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_passport2jo").getValue();
    var dateField = formContext.getAttribute("bcc_passport2uploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}

// For bcc_passport3
function onFileChangeForPassport31(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_passport3").getValue();
    var dateField = formContext.getAttribute("bcc_passport3uploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}

// For bcc_otherdocument
function onFileChangeForOtherDocument1(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_otherdocument").getValue();
    var dateField = formContext.getAttribute("bcc_otherdocumentuploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}

// For bcc_image
function onFileChangeForImage1(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_image").getValue();
    var dateField = formContext.getAttribute("bcc_photouploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}

// For bcc_degreecertificate
function onFileChangeForDegreeCertificate1(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_degreecertificate").getValue();
    var dateField = formContext.getAttribute("bcc_degreecertificateuploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}

// For bcc_degreecertificate2
function onFileChangeForDegreeCertificate21(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_degreecertificate2").getValue();
    var dateField = formContext.getAttribute("bcc_degreecertificate2uploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}

// For bcc_workerform
function onFileChangeForWorkerForm1(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_workerform").getValue();
    var dateField = formContext.getAttribute("bcc_workerformuploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}

// For bcc_cancellationform
function onFileChangeForCancellationForm1(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_cancellationform").getValue();
    var dateField = formContext.getAttribute("bcc_cancellationformuploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}






// For bcc_preapprovalworkpermit
function onFileChangeForPreApprovalWorkPermit(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_preapprovalworkpermit").getValue();
    var dateField = formContext.getAttribute("bcc_preapprovalworkpermitdate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}

// For bcc_signedjoboffer
function onFileChangeForSignedJobOffer(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_signedjoboffer").getValue();
    var dateField = formContext.getAttribute("bcc_signedjobofferdate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}

// For bcc_joboffer
function onFileChangeForJobOffer(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_joboffer").getValue();
    var dateField = formContext.getAttribute("bcc_jobofferdate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}

// For bcc_employeecontract
function onFileChangeForEmployeeContract(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_employeecontract").getValue();
    var dateField = formContext.getAttribute("bcc_employeecontractdocdate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}


function onCancelDocUpload(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_canceldocument").getValue();
    var dateField = formContext.getAttribute("bcc_canceldocumentuploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}














var Approval = {

    onChange: function (executionContext) {
        debugger;
        if (!executionContext) {
            return;
        }

        var formContext = executionContext.getFormContext();
        if (!formContext) {
            return;
        }

        var attr = formContext.getAttribute("bcc_approval");
        if (!attr) {
            return;
        }

        var value = attr.getValue();

        // 755700001 = Rejected
        if (value === 755700001) {

            // Check if record is saved
            var recordId = formContext.data.entity.getId();
            if (!recordId) {
                Xrm.Navigation.openAlertDialog({
                    title: "Save Required",
                    text: "Please save the record before rejecting."
                }).then(function () {
                    debugger;
                    // Revert the approval field
                    attr.setValue(null);
                });
                return;
            }

            Approval.showRejectDialog(formContext);

            // Set required level
            var remarkAttr = formContext.getAttribute("bcc_rejectremark");
            if (remarkAttr) {
                remarkAttr.setRequiredLevel("required");
            }

        } else {
            // Remove required level when not rejected
            var remarkAttr = formContext.getAttribute("bcc_rejectremark");
            if (remarkAttr) {
                remarkAttr.setRequiredLevel("none");
            }
        }
    },

    showRejectDialog: function (formContext) {
        debugger;

        var recordId = formContext.data.entity.getId();
        if (!recordId) return;

        recordId = recordId.replace(/[{}]/g, "");
        var entityName = formContext.data.entity.getEntityName();

        var html = `
        <html>
        <head>
            <style>
                html, body {
                    width:100%;
                    height:100%;
                    margin:0;
                    padding:0;
                    font-family: Segoe UI, Arial;
                    background:#f3f6f9;
                }
                body{
                    display:flex;
                    align-items:center;
                    justify-content:center;
                }
                .card{
                    width:320px;
                    background:#fff;
                    border-radius:8px;
                    box-shadow:0 6px 18px rgba(0,0,0,.15);
                    padding:24px;
                }
                h3{
                    margin:0 0 20px;
                    text-align:center;
                    font-size:17px;
                    color:#323130;
                    font-weight:600;
                }
                label{
                    font-size:13px;
                    color:#323130;
                    font-weight:600;
                    margin-top:16px;
                    display:block;
                }
                .required{
                    color:#d13438;
                    margin-left:3px;
                }
                textarea{
                    width:100%;
                    min-height:90px;
                    padding:8px;
                    font-size:13px;
                    margin-top:6px;
                    font-family:Segoe UI;
                    border:1px solid #8a8886;
                    border-radius:4px;
                    box-sizing:border-box;
                    resize:vertical;
                }
                textarea:focus{
                    outline:none;
                    border-color:#0078d4;
                }
                input[type="file"]{
                    width:100%;
                    padding:8px;
                    font-size:13px;
                    margin-top:6px;
                    border:1px solid #8a8886;
                    border-radius:4px;
                    box-sizing:border-box;
                }
                input[type="file"]:focus{
                    outline:none;
                    border-color:#0078d4;
                }
                .btn-container{
                    margin-top:24px;
                    display:flex;
                    justify-content:flex-end;
                    gap:10px;
                }
                button{
                    min-width:80px;
                    padding:8px 16px;
                    font-size:13px;
                    border:none;
                    border-radius:4px;
                    cursor:pointer;
                    font-family:Segoe UI;
                }
                .btn-cancel{
                    background:#f3f2f1;
                    color:#323130;
                }
                .btn-cancel:hover{
                    background:#edebe9;
                }
                .btn-submit{
                    background:#0078d4;
                    color:#fff;
                }
                .btn-submit:hover{
                    background:#106ebe;
                }
                .btn-submit:disabled{
                    background:#c8c6c4;
                    cursor:not-allowed;
                }
            </style>
        </head>

        <body>
            <div class="card">
                <h3>Rejection Details</h3>

                <label>Reason for Rejection <span class="required">*</span></label>
                <textarea id="rejectRemarks" placeholder="Enter rejection remarks..."></textarea>

                <label>Attach Rejection Document <span class="required">*</span></label>
                <input type="file" id="rejectFile"/>

                <div class="btn-container">
                    <button class="btn-cancel" onclick="cancelReject()">Cancel</button>
                    <button class="btn-submit" onclick="submitReject()">Submit</button>
                </div>
            </div>

            <script>
                function cancelReject(){
                    debugger;
                    window.opener.postMessage({ action: 'cancel' }, "*");
                    window.close();
                }

                function submitReject(){
                    debugger;
                    var remarks = document.getElementById('rejectRemarks').value;
                    var file = document.getElementById('rejectFile').files[0];

                    if(!remarks || !remarks.trim()){
                        alert("Please enter rejection remarks");
                        return;
                    }

                    if(!file){
                        alert("Please attach a rejection document");
                        return;
                    }

                    // Disable submit button
                    var btn = event.target;
                    btn.disabled = true;
                    btn.textContent = "Submitting...";

                    var reader = new FileReader();
                    reader.onload = function(){
                        debugger;
                        window.opener.postMessage({
                            action: 'submit',
                            remarks: remarks,
                            fileName: file.name,
                            fileBuffer: reader.result
                        }, "*");
                        window.close();
                    };
                    reader.onerror = function(){
                        debugger;
                        alert("Error reading file. Please try again.");
                        btn.disabled = false;
                        btn.textContent = "Submit";
                    };
                    reader.readAsArrayBuffer(file);
                }
            </script>
        </body>
        </html>`;

        var popup = window.open(
            "",
            "rejectPopup",
            "width=380,height=320,resizable=no,scrollbars=no"
        );

        if (!popup) {
            Xrm.Navigation.openAlertDialog({
                title: "Popup Blocked",
                text: "Please allow popups for this site and try again."
            });
            return;
        }

        popup.document.write(html);
        popup.document.close();

        window.addEventListener("message", function (e) {
            debugger;
            if (!e.data || !e.data.action) return;

            if (e.data.action === 'cancel') {
                // Reset approval field
                var approvalAttr = formContext.getAttribute("bcc_approval");
                if (approvalAttr) {
                    approvalAttr.setValue(null);
                }

                // Remove required level
                var remarkAttr = formContext.getAttribute("bcc_rejectremark");
                if (remarkAttr) {
                    remarkAttr.setRequiredLevel("none");
                }
                return;
            }

            if (e.data.action === 'submit') {

                if (!e.data.remarks || !e.data.fileBuffer) return;

                // Set rejection remarks
                var remarkAttr = formContext.getAttribute("bcc_rejectremark");
                if (remarkAttr) {
                    remarkAttr.setValue(e.data.remarks);
                    remarkAttr.setSubmitMode("always");
                }

                // Save the form first, then upload the file
                formContext.data.save().then(function () {
                    debugger;

                    uploadFileToDataverse(
                        entityName,
                        recordId,
                        "bcc_rejectdoc1",
                        e.data.fileName,
                        e.data.fileBuffer,
                        formContext
                    );

                }).catch(function (error) {
                    debugger;
                    Xrm.Navigation.openAlertDialog({
                        title: "Save Failed",
                        text: "Could not save the record: " + error.message
                    });
                });
            }

        }, { once: true });
    }
};


/* ============================= */
/* FILE UPLOAD FUNCTION          */
/* ============================= */

function uploadFileToDataverse(
    entityName,
    recordId,
    columnName,
    fileName,
    arrayBuffer,
    formContext
) {

    Xrm.Utility.getEntityMetadata(entityName).then(function (entityData) {
        debugger;

        var entitySetName = entityData.EntitySetName;

        var url =
            Xrm.Utility.getGlobalContext().getClientUrl() +
            `/api/data/v9.2/${entitySetName}(${recordId})/${columnName}?x-ms-file-name=${encodeURIComponent(fileName)}`;

        var req = new XMLHttpRequest();
        req.open("PATCH", url, true);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/octet-stream");
        req.setRequestHeader("Content-Disposition", `attachment; filename="${fileName}"`);
        req.setRequestHeader("x-ms-file-name", encodeURIComponent(fileName));

        req.onload = function () {
            debugger;
            if (req.status === 204 || req.status === 200) {

                // Refresh the file control and form immediately
                setTimeout(function () {
                    debugger;

                    // Force a complete form refresh to load the file
                    formContext.data.refresh(true).then(function () {
                        debugger;

                        // After form refresh, refresh the file control specifically
                        var fileCtrl = formContext.getControl(columnName);
                        if (fileCtrl && typeof fileCtrl.refresh === 'function') {
                            debugger;
                            fileCtrl.refresh();
                        }

                        Xrm.Navigation.openAlertDialog({
                            title: "Success",
                            text: "Rejection details submitted successfully! The document has been uploaded. ðŸ‘"
                        });

                    }).catch(function (refreshError) {
                        debugger;
                        // If refresh fails, still show success
                        Xrm.Navigation.openAlertDialog({
                            title: "Success",
                            text: "Rejection details submitted! Please refresh the form to see the document."
                        });
                    });

                }, 1000);

            } else {
                Xrm.Navigation.openAlertDialog({
                    title: "Upload Failed",
                    text: "Error: " + req.status + " - " + req.responseText
                });
            }
        };

        req.onerror = function () {
            debugger;
            Xrm.Navigation.openAlertDialog({
                title: "Upload Error",
                text: "Network error occurred while uploading the file."
            });
        };

        req.send(arrayBuffer);

    }).catch(function (error) {
        debugger;
        Xrm.Navigation.openAlertDialog({
            title: "Metadata Error",
            text: "Could not retrieve entity metadata: " + error.message
        });
    });
}







function setRejectDocMandatory(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();

    var approvalAttr = formContext.getAttribute("bcc_approval");
    var rejectDocAttr = formContext.getAttribute("bcc_rejectdoc1");

    if (!approvalAttr || !rejectDocAttr) return;

    var approvalValue = approvalAttr.getValue();

    if (approvalValue === 755700001) {
        rejectDocAttr.setRequiredLevel("required");
    } else {
        rejectDocAttr.setRequiredLevel("none");
    }
}






// For bcc_insurancemolattachment
function onFileChangeForInsuranceMolAttachment(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_insurancemolattachment").getValue();
    var dateField = formContext.getAttribute("bcc_insurancemolattachmentuploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}

// For bcc_labourfee
function onFileChangeForLabourFee(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_labourfee").getValue();
    var dateField = formContext.getAttribute("bcc_labourfeeuploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}

// For bcc_visafee
function onFileChangeForVisaFee(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_visafee").getValue();
    var dateField = formContext.getAttribute("bcc_visafeeuploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}

// For bcc_changestatus
function onFileChangeForChangeStatus(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_changestatus").getValue();
    var dateField = formContext.getAttribute("bcc_changestausuploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}

// For bcc_medicalfee
function onFileChangeForMedicalFee(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_medicalfee").getValue();
    var dateField = formContext.getAttribute("bcc_medicalfeeuploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}

// For bcc_eid
function onFileChangeForEid(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_eid").getValue();
    var dateField = formContext.getAttribute("bcc_eiduploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}

// For bcc_tawjeetraining
function onFileChangeForTawjeeTraining(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_tawjeetraining").getValue();
    var dateField = formContext.getAttribute("bcc_tawjeetraininguploaddate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}

// For bcc_residencedocument
function onFileChangeForResidenceDocument(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var fileValue = formContext.getAttribute("bcc_residencedocument").getValue();
    var dateField = formContext.getAttribute("bcc_signcontractdocdate");

    if (fileValue != null && fileValue != "") {
        dateField.setValue(new Date());
    } else {
        dateField.setValue(null);
    }
}








//  Register BPF stage change event on form load
function OnLoad(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();

    if (formContext.data.process) {
        formContext.data.process.addOnStageChange(function (executionContext) {
            debugger;
            OnBPFStageChange(executionContext);
        });
    }
}

//  Fires when user clicks Next Stage in BPF
function OnBPFStageChange(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var subStageAttr = formContext.getAttribute("bcc_substagestatus");
    if (!subStageAttr) return;

    var targetStages = [
        "Insurance MOL Form",
        "Labour Fee",
        "Visa Fee",
        "EID Fee",
        "IOLE Fee",
        "Tawjee Fee",
        "Residency"
    ];

    var activeStageName = formContext.data.process.getActiveStage().getName();
    if (targetStages.indexOf(activeStageName) === -1) return;

    subStageAttr.setValue(755700000); // PENDING_PRO
    formContext.data.save();
}

// Your original code â€” untouched
function OnFileChange(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();

    setTimeout(function () {
        debugger;
        if (formContext.data.getIsDirty()) {

        }
    }, 1200);
}

function OnUploadOfDOC(executionContext) {
    debugger;

    var formContext = executionContext.getFormContext();
    var subStageAttr = formContext.getAttribute("bcc_substagestatus");
    if (!subStageAttr) return;

    var PENDING_PRO = 755700000;
    var PENDING_HR = 755700001;
    var COMPLETED = 755700002;

    var approval = formContext.getAttribute("bcc_approval")?.getValue();
    var preApproval = formContext.getAttribute("bcc_preapprovalworkpermit")?.getValue();

    var jobOffer = formContext.getAttribute("bcc_joboffer")?.getValue();
    var empContract = formContext.getAttribute("bcc_employeecontract")?.getValue();

    var signedJob = formContext.getAttribute("bcc_signedjoboffer")?.getValue();
    var signedEmp = formContext.getAttribute("bcc_signedemployeecontract")?.getValue();

    var visaFee = formContext.getAttribute("bcc_visafee")?.getValue();
    var ChangeStatus = formContext.getAttribute("bcc_changestatus")?.getValue();

    var medicalCert = formContext.getAttribute("bcc_medicalcertificate")?.getValue();
    var medicalResult = formContext.getAttribute("bcc_medicalresult")?.getValue();
    var tawajee = formContext.getAttribute("bcc_tawjiitrainingdocument")?.getValue();
    var medicalfee = formContext.getAttribute("bcc_medicalfee")?.getValue();
    var residency = formContext.getAttribute("bcc_residencedocument")?.getValue();
    var eid = formContext.getAttribute("bcc_eid")?.getValue();

    //  Residency + EID â†’ COMPLETED
    if (residency) {
        subStageAttr.setValue(COMPLETED);
        return;
    }
    if (eid) {
        subStageAttr.setValue(COMPLETED);
    }

    // Medical Result + Tawajee â†’ PENDING PRO
    if (medicalResult) {
        subStageAttr.setValue(PENDING_PRO);
        return;
    }
    if (tawajee) {
        subStageAttr.setValue(PENDING_PRO);
        return;
    }

    //  Medical Certificate â†’ PENDING PRO
    if (medicalCert) {
        subStageAttr.setValue(PENDING_PRO);
        return;
    }

    //  Visa Fee + ChangeStatus â†’ PENDING HR
    if (visaFee) {
        subStageAttr.setValue(PENDING_HR);
        return;
    }
    if (ChangeStatus) {
        subStageAttr.setValue(PENDING_HR);
    }

    //  Signed Job + Signed Contract â†’ PENDING PRO
    if (signedJob && signedEmp) {
        subStageAttr.setValue(PENDING_PRO);
        return;
    }

    //  Job Offer + Employee Contract â†’ PENDING HR
    if (jobOffer && empContract) {
        subStageAttr.setValue(PENDING_HR);
        return;
    }

    //  Pre-Approval Work Permit â†’ PENDING PRO
    if (preApproval) {
        subStageAttr.setValue(PENDING_PRO);
        return;
    }
    if (medicalfee == null) {
        subStageAttr.setValue(PENDING_PRO);
        return;
    }
    if (medicalfee !== null && medicalResult == null) {
        subStageAttr.setValue(PENDING_HR);
        return;
    }
    if (medicalResult == null) {
        subStageAttr.setValue(PENDING_HR);
        return;
    }

    //  Approval = Approved â†’ PENDING PRO
    if (approval === 755700000) {
        subStageAttr.setValue(PENDING_PRO);
        return;
    }
}




function onBPFStageChangetocompletee(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();

    var activeStage = formContext.data.process.getActiveStage();
    if (activeStage === null) return;

    var activeStageName = activeStage.getName();
    console.log("Active Stage: " + activeStageName);

    if (activeStageName !== "Employer Verification") return;

    var processStatus = formContext.data.process.getStatus();
    console.log("Process Status: " + processStatus);

    var employerVerification = formContext.getAttribute("bcc_employerverification");
    if (employerVerification === null) return;

    var employerVerificationValue = employerVerification.getValue();
    console.log("Employer Verification Value: " + employerVerificationValue);

    if (employerVerificationValue !== 755700000) return;

    var recordId = formContext.data.entity.getId().replace("{", "").replace("}", "");
    var entityName = formContext.data.entity.getEntityName();

    Xrm.WebApi.updateRecord(entityName, recordId, {
        "statuscode": 755700001,
        "statecode": 0
    }).then(
        function success() {
            debugger;
            console.log("Status Reason updated to Completed successfully.");
            setTimeout(function () {
                debugger;
                window.location.reload();
            }, 1500);
        },
        function error(err) {
            debugger;
            console.error("Error: " + err.message);
            Xrm.Navigation.openAlertDialog({ text: "Error: " + err.message });
        }
    );
}









function setStatusOnStageChanges(executionContext) {
    debugger;

    var formContext = executionContext.getFormContext();

    if (!formContext.data.process) {
        console.log("No BPF found.");
        return;
    }

    formContext.data.process.addOnStageChange(function (stageContext) {
        debugger;

        try {

            var activeStage = formContext.data.process.getActiveStage();

            if (!activeStage) return;

            var stageName = activeStage.getName();
            console.log("Current Stage: " + stageName);

            //  When user moves to Employer Verification
            if (stageName === "Employer Verification") {

                var statusAttr = formContext.getAttribute("statuscode");
                if (!statusAttr) {
                    console.log("statuscode field not found.");
                    return;
                }

                // Example: set Status Reason value
                // Replace 100000002 with your actual option set value
                var newStatusValue = 100000001;

                if (statusAttr.getValue() !== newStatusValue) {

                    statusAttr.setValue(newStatusValue);
                    statusAttr.setSubmitMode("always");

                    formContext.data.save().then(
                        function () {
                            debugger;
                            console.log("Status Reason updated successfully.");
                        },
                        function (error) {
                            debugger;
                            console.log("Error saving form: " + error.message);
                        }
                    );
                }
            }

        } catch (e) {
            console.log("Error: " + e.message);
        }

    });
}




function onStatusCodeChange(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var statusCodeValue = formContext.getAttribute("statuscode").getValue();

    if (statusCodeValue === 100000000) {

        var entityName = formContext.data.entity.getEntityName();

        // Fix - properly clean the GUID
        var rawId = formContext.data.entity.getId();
        var entityId = rawId.replace(/[{}]/g, "").trim();

        alert("Entity: " + entityName + " | ID: " + entityId);

        Xrm.WebApi.updateRecord(entityName, entityId, {
            "statecode": 1,
            "statuscode": 100000000
        }).then(
            function () {
                debugger;
                alert("SUCCESS - Record Deactivated!");
                formContext.data.refresh(false);
            },
            function (error) {
                debugger;
                alert("FAILED - " + error.message);
            }
        );
    }
}


function setStatusOnBPFCompletess(executionContext) {
    debugger;

    var formContext = executionContext.getFormContext();

    // ----- CHECK CORRECT FORM -----
    var currentForm = formContext.ui.formSelector.getCurrentItem();
    if (!currentForm) return;

    var currentFormId = currentForm.getId().replace(/[{}]/g, "").toLowerCase();
    var targetFormId = "ea396a4e-31e6-f011-8406-70a8a52030e9";

    if (currentFormId !== targetFormId) {
        return; // Stop if not target form
    }

    // ----- CHECK BPF EXISTS -----
    if (!formContext.data.process) {
        console.log("No Business Process Flow found.");
        return;
    }

    // ----- REGISTER PROCESS STATUS CHANGE EVENT -----
    formContext.data.process.addOnProcessStatusChange(function () {
        debugger;

        try {

            var processStatus = formContext.data.process.getStatus();
            console.log("Process Status: " + processStatus);

            // When BPF is finished
            if (processStatus === "finished") {

                var statusAttr = formContext.getAttribute("statuscode");

                if (!statusAttr) {
                    console.log("statuscode field not found.");
                    return;
                }

                var currentStatus = statusAttr.getValue();
                console.log("Current Status Reason: " + currentStatus);

                // 100000001 = Hired
                if (currentStatus !== 100000002) {

                    statusAttr.setValue(100000002);
                    statusAttr.setSubmitMode("always");

                    formContext.data.save().then(
                        function () {
                            debugger;
                            console.log("Status updated to Hired successfully.");
                        },
                        function (error) {
                            debugger;
                            console.log("Error saving form: " + error.message);
                        }
                    );
                }
            }

        } catch (e) {
            console.log("Error in BPF status change: " + e.message);
        }

    });
}

function setStatusOnBPFCompletes(executionContext) {
    debugger;

    var formContext = executionContext.getFormContext();

    // ----- CHECK CORRECT FORM -----
    var currentForm = formContext.ui.formSelector.getCurrentItem();
    if (!currentForm) return;

    var currentFormId = currentForm.getId().replace(/[{}]/g, "").toLowerCase();
    var targetFormId = "ea396a4e-31e6-f011-8406-70a8a52030e9";

    if (currentFormId !== targetFormId) {
        return;
    }

    // ----- CHECK BPF EXISTS -----
    if (!formContext.data.process) {
        console.log("No Business Process Flow found.");
        return;
    }

    // ----- REGISTER PROCESS STATUS CHANGE EVENT -----
    formContext.data.process.addOnProcessStatusChange(function () {
        debugger;

        try {

            var processStatus = formContext.data.process.getStatus();
            console.log("Process Status: " + processStatus);

            // When BPF is finished
            if (processStatus === "finished") {

                var stateAttr = formContext.getAttribute("statecode");
                var statusAttr = formContext.getAttribute("statuscode");

                if (!stateAttr || !statusAttr) {
                    console.log("State or Status field not found.");
                    return;
                }

                var currentState = stateAttr.getValue();
                var currentStatus = statusAttr.getValue();

                console.log("Current State: " + currentState);
                console.log("Current Status: " + currentStatus);

                //  Assuming:
                // statecode 0 = Active
                // statecode 1 = Inactive
                // 100000002 = Hired (Inactive state)

                if (currentState !== 1 || currentStatus !== 100000002) {

                    //  Set State FIRST
                    stateAttr.setValue(1);

                    //  Then set Status Reason
                    statusAttr.setValue(100000002);

                    stateAttr.setSubmitMode("always");
                    statusAttr.setSubmitMode("always");

                    formContext.data.save().then(
                        function () {
                            debugger;
                            console.log("State and Status updated to Hired successfully.");
                        },
                        function (error) {
                            debugger;
                            console.log("Error saving form: " + error.message);
                        }
                    );
                }
            }

        } catch (e) {
            console.log("Error in BPF status change: " + e.message);
        }

    });
}




var BPFControlvisafee = (function () {
    debugger;
    var visaFeeStageName = "Visa Fee";
    var targetFormId = "5b8da095-5ef0-f011-8406-70a8a52260b1".toLowerCase();

    // Check if BPF is fully completed
    function isBPFCompleted(formContext) {
        debugger;
        try {
            var process = formContext.data.process;
            if (!process) return false;
            var status = process.getStatus(); // "active", "finished", "aborted"
            return status === "finished";
        } catch (e) {
            return false;
        }
    }

    // Check if current stage is Visa Fee or beyond using getActivePath()
    function isVisaFeeOrBeyond(formContext) {
        debugger;
        try {
            var process = formContext.data.process;
            if (!process) return false;

            var activeStage = process.getActiveStage();
            if (!activeStage) return false;

            var activeStageName = activeStage.getName();

            // If current stage is exactly Visa Fee
            if (activeStageName === visaFeeStageName) return true;

            // Check if Visa Fee was already passed using getActivePath()
            // getActivePath() returns all stages from start up to and including active stage
            var activePath = process.getActivePath();
            if (!activePath) return false;

            var visaFeeFound = false;
            for (var i = 0; i < activePath.getLength(); i++) {
                var stage = activePath.get(i);
                if (stage.getName() === visaFeeStageName) {
                    visaFeeFound = true;
                    break;
                }
            }
            return visaFeeFound;
        } catch (e) {
            console.warn("BPFControlvisafee: isVisaFeeOrBeyond error.", e);
            return false;
        }
    }

    // Hide all form options except the current one (blocks form switching)
    function lockFormSwitcher(formContext) {
        debugger;
        try {
            var formSelector = formContext.ui.formSelector;
            var currentFormId = formSelector.getCurrentItem().getId().toLowerCase();
            var items = formSelector.items;
            if (!items) return;

            items.forEach(function (item) {
                debugger;
                if (item.getId().toLowerCase() !== currentFormId) {
                    item.setVisible(false);
                }
            });
        } catch (e) {
            console.warn("BPFControlvisafee: Could not lock form switcher.", e);
        }
    }

    // Show all forms (unlock when BPF is complete)
    function unlockFormSwitcher(formContext) {
        debugger;
        try {
            var formSelector = formContext.ui.formSelector;
            var items = formSelector.items;
            if (!items) return;

            items.forEach(function (item) {
                debugger;
                item.setVisible(true);
            });
        } catch (e) {
            console.warn("BPFControlvisafee: Could not unlock form switcher.", e);
        }
    }

    function onLoad(executionContext) {
        debugger;
        var formContext = executionContext.getFormContext();
        var process = formContext.data.process;
        if (!process) return;

        // On load â€” if BPF is complete, unlock everything and stop
        if (isBPFCompleted(formContext)) {
            unlockFormSwitcher(formContext);
            return;
        }

        // On load â€” if already at Visa Fee or beyond, lock form switcher immediately
        if (isVisaFeeOrBeyond(formContext)) {
            lockFormSwitcher(formContext);
        }

        // Block Previous stage navigation when at Visa Fee stage
        process.addOnPreStageChange(function (stageContext) {
            debugger;
            var eventArgs = stageContext.getEventArgs();
            var direction = eventArgs.getDirection();
            var activeStage = process.getActiveStage();
            if (!activeStage) return;

            var activeStageName = activeStage.getName();
            var currentFormId = formContext.ui.formSelector
                .getCurrentItem()
                .getId()
                .toLowerCase();

            if (activeStageName === visaFeeStageName &&
                currentFormId === targetFormId &&
                direction === "Previous") {
                eventArgs.preventDefault();
                Xrm.Navigation.openAlertDialog({
                    text: "You cannot move to previous stages after reaching Visa Fee."
                });
            }
        });

        // When stage changes â€” check if Visa Fee reached â†’ lock, or BPF complete â†’ unlock
        process.addOnStageChange(function () {
            debugger;
            if (isBPFCompleted(formContext)) {
                unlockFormSwitcher(formContext);
            } else if (isVisaFeeOrBeyond(formContext)) {
                lockFormSwitcher(formContext);
            }
        });
    }

    return {
        onLoad: onLoad
    };
})();







function moveToMOLVerificationStage(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var empVerAttr = formContext.getAttribute("bcc_employerverification");

    if (!empVerAttr) {

        return;
    }

    var empVerValue = empVerAttr.getValue();


    if (empVerValue === 755700001) {

        //  Set statuscode to 100000000
        var statusAttr = formContext.getAttribute("statuscode");
        if (statusAttr) {
            statusAttr.setValue(100000000);

        } else {

        }

        //  SAVE FIRST to clear dirty form, THEN move stage
        formContext.data.save().then(function () {
            debugger;


            var activeProcess = formContext.data.process.getActiveProcess();

            if (!activeProcess) {

                return;
            }

            var stageCollection = activeProcess.getStages().get();
            var targetStage = null;
            var allStageNames = "";

            for (var i = 0; i < stageCollection.length; i++) {
                var stage = stageCollection[i];
                allStageNames += "\n - " + stage.getName() + " (ID: " + stage.getId() + ")";
                if (stage.getName() === "MOL Verfication") {
                    targetStage = stage;
                }
            }



            if (!targetStage) {

                return;
            }



            var currentStage = formContext.data.process.getActiveStage();


            //  Now move backwards â€” form is clean so dirtyForm error won't happen
            function moveToPreviousStageById(targetStageId, callback) {
                debugger;
                var currentStage = formContext.data.process.getActiveStage();

                if (!currentStage) {

                    callback("error: no active stage");
                    return;
                }



                if (currentStage.getId().toLowerCase() === targetStageId.toLowerCase()) {

                    callback("success");
                    return;
                }

                formContext.data.process.movePrevious(function (result) {
                    debugger;

                    if (result === "success") {
                        moveToPreviousStageById(targetStageId, callback);
                    } else {

                        callback("error: movePrevious failed");
                    }
                });
            }

            moveToPreviousStageById(targetStage.getId(), function (result) {
                debugger;
                if (result === "success") {

                    formContext.data.save();
                } else {

                }
            });

        }).catch(function (error) {
            debugger;
            alert(" ERROR: Form save failed!\nReason: " + (error && error.message ? error.message : JSON.stringify(error)));
        });
    }
}









async function setManpowerQuotaFromJobApplication(executionContext) {
    debugger;
    const formContext = executionContext.getFormContext();

    // Step 1: Get the Job Application lookup value from Job Offer
    const jobAppLookup = formContext.getAttribute("bcc_jobapplication").getValue();

    if (!jobAppLookup || jobAppLookup.length === 0) {
        return;
    }

    const jobAppId = jobAppLookup[0].id.replace("{", "").replace("}", "");

    try {
        // Step 2: Retrieve Job Application record and expand bcc_DesignationAsPerVisa
        const result = await Xrm.WebApi.retrieveRecord(
            "bcc_jobapplication",
            jobAppId,
            "?$select=_bcc_designationaspervisa_value&$expand=bcc_DesignationAsPerVisa($select=bcc_manpowerquotaid,bcc_designationasperquota)"
        );

        // Step 3: Read the expanded designation value
        const designation = result["bcc_DesignationAsPerVisa"];

        if (!designation || !designation["bcc_manpowerquotaid"]) {
            // null â€” do nothing
            return;
        }

        // Step 4: Set it into bcc_manpowerquota on Job Offer form
        const lookupValue = [
            {
                id: designation["bcc_manpowerquotaid"],
                name: designation["bcc_designationasperquota"],
                entityType: "bcc_manpowerquota"
            }
        ];

        formContext.getAttribute("bcc_manpowerquota").setValue(lookupValue);

    } catch (error) {
        console.error("Error retrieving Job Application record:", error.message);
    }
}



var isAdvancing = false;

function autoAdvanceBPF(executionContext) {
    debugger;
    if (isAdvancing) return;

    // Block immediately before anything else
    isAdvancing = true;

    var formContext = executionContext.getFormContext();

    var LAST_STAGE_GUID = "3089ca34-0fa6-4048-9b0a-bcf107039f50";

    var validStageGuids = [
        "16e653d2-add6-46af-859a-eee0f54f399b",
        "df56003a-5d95-4faa-9fcc-427bb1a5a573",
        "af6db8ad-18b6-46e8-bdc8-f910a3eddd46",
        "cd4cfbe7-2cd0-457a-b693-52f9d1f930bf",
        "119a023d-1485-45fc-9b58-e18d8652dba4",
        "0567e092-2b2e-4988-838b-cf84d352ae00",
        "dff74fb6-fe45-4c07-97b6-d217650772d6",
        "b99a134e-f902-45c1-b212-1838eb44ad49",
        "b182d851-a82c-4b85-bba5-240cbcab41ea",
        "9a7778a2-96d3-4b7c-a77e-00e5e921f6ff",
        "89d34cfa-b743-45a9-b6c2-55a033e5677c",
        "3089ca34-0fa6-4048-9b0a-bcf107039f50"
    ];

    var stageFormMap = {
        "16e653d2-add6-46af-859a-eee0f54f399b": "033cc592-0d62-421a-861c-c06f1e5c572d",
        "af6db8ad-18b6-46e8-bdc8-f910a3eddd46": "033cc592-0d62-421a-861c-c06f1e5c572d",
        "df56003a-5d95-4faa-9fcc-427bb1a5a573": "778573dd-2ce6-f011-8406-70a8a51e50d4",
        "cd4cfbe7-2cd0-457a-b693-52f9d1f930bf": "91db6c01-e2ea-f011-8406-70a8a51d70b3",
        "119a023d-1485-45fc-9b58-e18d8652dba4": "2475379f-5df0-f011-8406-70a8a52030e9",
        "0567e092-2b2e-4988-838b-cf84d352ae00": "5b8da095-5ef0-f011-8406-70a8a52260b1",
        "dff74fb6-fe45-4c07-97b6-d217650772d6": "eb0903e4-5ef0-f011-8406-70a8a52030e9",
        "b99a134e-f902-45c1-b212-1838eb44ad49": "1eb03537-5ff0-f011-8406-70a8a522b0a0",
        "b182d851-a82c-4b85-bba5-240cbcab41ea": "080ee06f-5ff0-f011-8406-0022480de9a8",
        "9a7778a2-96d3-4b7c-a77e-00e5e921f6ff": "80cc2fc5-5ff0-f011-8406-0022480de9a8",
        "89d34cfa-b743-45a9-b6c2-55a033e5677c": "874e5278-60f0-f011-8406-70a8a52260b1",
        "3089ca34-0fa6-4048-9b0a-bcf107039f50": "76375402-61f0-f011-8406-70a8a522b0a0"
    };

    try {
        var activeStage = formContext.data.process.getActiveStage();
        if (!activeStage) { isAdvancing = false; return; }

        var currentStageId = activeStage.getId().toLowerCase();

        if (currentStageId === LAST_STAGE_GUID.toLowerCase()) { isAdvancing = false; return; }

        var isValidStage = validStageGuids.some(function (guid) {
            debugger;
            return guid.toLowerCase() === currentStageId;
        });

        if (!isValidStage) { isAdvancing = false; return; }

        formContext.data.process.moveNext(function (result) {
            debugger;
            if (result === "success") {

                var nextStage = formContext.data.process.getActiveStage();
                var nextStageId = nextStage.getId().toLowerCase();
                var nextFormId = stageFormMap[nextStageId];

                if (nextFormId) {
                    var recordId = formContext.data.entity.getId();
                    var entityName = formContext.data.entity.getEntityName();

                    Xrm.Navigation.openForm({
                        entityName: entityName,
                        entityId: recordId,
                        formId: nextFormId
                    }).then(function () {
                        debugger;
                        isAdvancing = false;
                    }, function () {
                        debugger;
                        isAdvancing = false;
                    });
                } else {
                    isAdvancing = false;
                }
            } else {
                isAdvancing = false;
            }
        });

    } catch (e) {
        isAdvancing = false;
    }
}





function onApprovalChangee(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var approvalField = formContext.getAttribute("bcc_approval");
    if (approvalField == null) return;
    var approvalValue = approvalField.getValue();

    if (approvalValue === 755700001) {
        // REJECT â€” Next/Finish button hide karo
        hideProcessButtons();
    } else {
        // APPROVE â€” buttons wapas dikhao
        showProcessButtons();
    }
}

function hideProcessButtons() {
    debugger;
    var style = document.getElementById("bcc-process-style");
    if (!style) {
        style = document.createElement("style");
        style.id = "bcc-process-style";
        document.head.appendChild(style);
    }
    style.innerHTML = ".processButton, button[data-id='moveNextStage'], button[data-id='finish'] { display: none !important; }";
}

function showProcessButtons() {
    debugger;
    var style = document.getElementById("bcc-process-style");
    if (style) {
        style.innerHTML = "";
    }
}

function onFormmLoad(executionContext) {
    debugger;
    onApprovalChangee(executionContext);
}







function toggleRejectFieldsRequirementt(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();

    var approvalField = formContext.getAttribute("bcc_approval");
    if (!approvalField) return;

    var approvalValue = approvalField.getValue();

    var remarkField = formContext.getAttribute("bcc_rejectremark");
    var docField = formContext.getAttribute("bcc_rejectdoc1");

    if (!remarkField || !docField) return;

    // If approval = 755700001 â†’ make mandatory
    if (approvalValue === 755700001) {
        remarkField.setRequiredLevel("required");
        docField.setRequiredLevel("required");
    }
    // If approval = 755700000 â†’ make optional
    else if (approvalValue === 755700000) {
        remarkField.setRequiredLevel("none");
        docField.setRequiredLevel("none");
    }
    // Optional: handle null or other values
    else {
        remarkField.setRequiredLevel("none");
        docField.setRequiredLevel("none");
    }
}









function BPFStageFormHandler(executionContext) {
    debugger;

    var formContext = executionContext.getFormContext();
    if (!formContext.data.process) return;

    function switchForm() {
        debugger;

        var stage = formContext.data.process.getActiveStage();
        if (!stage) return;

        var stageName = stage.getName();

        var INFORMATION_FORM_ID = "033cc592-0d62-421a-861c-c06f1e5c572d";
        var PREWORKPERMIT_FORM_ID = "778573dd-2ce6-f011-8406-70a8a51e50d4";

        var targetFormId = null;
        var statusValue = null; //  added
        var stateCode = formContext.getAttribute("statecode").getValue();
        if (stateCode === 1) return;

        //  Your exact logic (UNCHANGED, only status added)
        if (stageName === "Initialization" || stageName === "Approval") {
            targetFormId = INFORMATION_FORM_ID;
            statusValue = 100000000; //  Info form status
        }
        else if (stageName === "Pre Approval Work Permit") {
            targetFormId = PREWORKPERMIT_FORM_ID;
            statusValue = 100000001; //  Pre Work Permit status
        }

        if (!targetFormId) return;

        var currentForm = formContext.ui.formSelector.getCurrentItem();
        if (!currentForm) return;

        var currentFormId = currentForm.getId();

        //  Set statuscode (NEW LOGIC)
        var statusAttr = formContext.getAttribute("statuscode");
        if (statusAttr && statusAttr.getValue() !== statusValue) {
            statusAttr.setValue(statusValue);
            statusAttr.setSubmitMode("always");
        }

        //  Avoid unnecessary reload
        if (currentFormId.toLowerCase() === targetFormId.toLowerCase()) return;

        setTimeout(function () {
            debugger;

            var forms = formContext.ui.formSelector.items.get();

            for (var i = 0; i < forms.length; i++) {
                if (forms[i].getId().toLowerCase() === targetFormId.toLowerCase()) {
                    forms[i].navigate();
                    break;
                }
            }

        }, 500);
    }

    //  Prevent multiple bindings
    if (!formContext._stageHandlerAttached) {
        formContext.data.process.addOnStageChange(switchForm);
        formContext._stageHandlerAttached = true;
    }

    //  Handle save / refresh
    switchForm();
}






"use strict";

var BPFStageControl = BPFStageControl || {};

BPFStageControl.onLoad = function (executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();

    // Clear notifications on load
    formContext.ui.clearFormNotification("bcc_approval_block");

    // Watch bcc_approval field changes to clear notification when approved
    var approvalField = formContext.getAttribute("bcc_approval");
    if (approvalField) {
        approvalField.addOnChange(function () {
            debugger;
            var value = approvalField.getValue();
            if (value === 755700000) {
                formContext.ui.clearFormNotification("bcc_approval_block");
            }
        });
    }

    // Hook into the BPF Next Stage button
    BPFStageControl.hookBPFNextStageButton(formContext);
};

BPFStageControl.hookBPFNextStageButton = function (formContext) {
    debugger;
    try {
        var process = formContext.data.process;

        if (!process) {
            console.log("No BPF process found on this form.");
            return;
        }

        // Get active stage
        var activeStage = process.getActiveStage();
        if (!activeStage) {
            console.log("No active stage found.");
            return;
        }

        console.log("Active Stage ID: " + activeStage.getId());

        // Add OnStageSelected event (fires when user tries to move stage)
        process.addOnStageChange(function (executionContext) {
            debugger;
            BPFStageControl.handleStageChange(executionContext, formContext);
        });

        process.addOnPreStageChange(function (executionContext) {
            debugger;
            BPFStageControl.handlePreStageChange(executionContext, formContext);
        });

    } catch (e) {
        console.log("Error hooking BPF: " + e.message);
    }
};

BPFStageControl.handlePreStageChange = function (executionContext, formContext) {
    debugger;
    try {
        var eventArgs = executionContext.getEventArgs();
        var direction = eventArgs.getDirection(); // "Next" or "Previous"

        if (direction !== "Next") return; // Only block forward movement

        var approvalField = formContext.getAttribute("bcc_approval");

        if (!approvalField) {
            eventArgs.preventDefault();
            formContext.ui.setFormNotification(
                "Approval field (bcc_approval) not found. Cannot proceed.",
                "ERROR",
                "bcc_approval_block"
            );
            return;
        }

        var approvalValue = approvalField.getValue();

        if (approvalValue === 755700000) {
            // Approved â€” allow movement, navigate to target form
            formContext.ui.clearFormNotification("bcc_approval_block");
            BPFStageControl.navigateToTargetForm(formContext);

        } else if (approvalValue === 755700001) {
            // Rejected â€” block
            eventArgs.preventDefault();
            formContext.ui.setFormNotification(
                "Stage cannot be advanced. The Approval status is 'Not Approved'. Please get approval before proceeding.",
                "ERROR",
                "bcc_approval_block"
            );

        } else {
            // Null / not set â€” block
            eventArgs.preventDefault();
            formContext.ui.setFormNotification(
                "Please set the Approval field to 'Approved' before moving to the next stage.",
                "ERROR",
                "bcc_approval_block"
            );
        }

    } catch (e) {
        console.log("Error in handlePreStageChange: " + e.message);
    }
};

BPFStageControl.handleStageChange = function (executionContext, formContext) {
    debugger;
    try {
        var eventArgs = executionContext.getEventArgs();
        var newStage = eventArgs.getStage();

        if (!newStage) return;

        console.log("Stage changed to: " + newStage.getId());

        // If moved to next stage successfully, navigate to the target form
        BPFStageControl.navigateToTargetForm(formContext);

    } catch (e) {
        console.log("Error in handleStageChange: " + e.message);
    }
};

BPFStageControl.navigateToTargetForm = function (formContext) {
    debugger;
    try {
        var targetFormId = "778573dd-2ce6-f011-8406-70a8a51e50d4";

        var formSelector = formContext.ui.formSelector;
        if (!formSelector) {
            console.log("Form selector not available.");
            return;
        }

        var availableForms = formSelector.items.get();
        var targetForm = null;

        availableForms.forEach(function (form) {
            debugger;
            if (form.getId().toLowerCase() === targetFormId.toLowerCase()) {
                targetForm = form;
            }
        });

        if (targetForm) {
            console.log("Navigating to target form: " + targetFormId);
            targetForm.navigate();
        } else {
            console.log("Target form not found in available forms: " + targetFormId);
        }

    } catch (e) {
        console.log("Error navigating to form: " + e.message);
    }
};
