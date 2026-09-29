function onEntryTypeChange(executionContext) {
    var formContext = executionContext.getFormContext();

    if (formContext.ui.getFormType() > 1) {
        // Make all the Fields on the Form ready Only
        formContext.ui.controls.forEach(function (control) {
            try {
                if (control && control.getDisabled && control.setDisabled) {
                    control.setDisabled(true);
                }
            } catch (e) {
                console.warn(`Error locking control ${control.getName()}:`, e);
            }
        });
    }

    var entryTypeAttr = formContext.getAttribute("new_entrytype");
    var entryTypeValue = entryTypeAttr ? entryTypeAttr.getValue() : null;

    var absentReasonAttr = formContext.getAttribute("bcc_absentreason");
    var absentReasonValue = absentReasonAttr ? absentReasonAttr.getValue() : null;

    var absentReasonCtrl = formContext.getControl("bcc_absentreason");
    var overtime1Ctrl = formContext.getControl("new_overtime1");
    var overtime2Ctrl = formContext.getControl("new_overtime2");
    var overtime3Ctrl = formContext.getControl("new_overtime3");
    var totalHoursCtrl = formContext.getControl("bcc_total_hours");

    var overtime1Attr = formContext.getAttribute("new_overtime1");
    var overtime2Attr = formContext.getAttribute("new_overtime2");
    var overtime3Attr = formContext.getAttribute("new_overtime3");

    var totalHoursAttr = formContext.getAttribute("bcc_total_hours");
    var durationAttr = formContext.getAttribute("msdyn_duration");
    var newDurationAttr = formContext.getAttribute("bcc_newduration");

    // ABSENT CASE
    if (entryTypeValue === 100000001) {

        absentReasonCtrl.setVisible(true);

        overtime1Attr.setValue(null);
        overtime2Attr.setValue(null);
        overtime3Attr.setValue(null);
        totalHoursAttr.setValue(null);
        

        overtime1Ctrl.setDisabled(true);
        overtime2Ctrl.setDisabled(true);
        overtime3Ctrl.setDisabled(true);
        totalHoursCtrl.setDisabled(true);

        // ABSENT REASON (2 special values)
        if (absentReasonValue === 755700000) {

            durationAttr.setValue(480);      // 8 hours (minutes)
            durationAttr.setSubmitMode("always");

            newDurationAttr.setValue(8);     // hours
            newDurationAttr.setSubmitMode("always");

            totalHoursAttr.setValue(8);      // total hours
            totalHoursAttr.setSubmitMode("always");
        }
        else {
            durationAttr.setValue(null);
            newDurationAttr.setValue(null);
            totalHoursAttr.setValue(null);
        }
    }

    // PRESENT CASE
    else if (entryTypeValue === 100000000) {
        absentReasonCtrl.setVisible(false);

        overtime1Ctrl.setDisabled(false);
        overtime2Ctrl.setDisabled(false);
        overtime3Ctrl.setDisabled(false);
        totalHoursCtrl.setDisabled(false);

        
        absentReasonAttr.setValue(null);
    }
    else{
        absentReasonCtrl.setVisible(false);
    }
}




function onFormLoad(executionContext) {
    onEntryTypeChange(executionContext);

    var formContext = executionContext.getFormContext();
    try {
        const statusAttr = formContext.getAttribute("msdyn_entrystatus");
        if (!statusAttr) return;

        const statusValue = statusAttr.getValue();

        // --- Status Values ---
        const APPROVED_STATUS = 192350002;
        const PENDING_FOR_APPROVAL = 192350004;
        const CANCELLED = 192354320;

        // Check if record should be locked
        if (statusValue === APPROVED_STATUS ||
            statusValue === PENDING_FOR_APPROVAL ||
            statusValue === CANCELLED) {

            formContext.ui.controls.forEach(function (control) {
                try {
                    if (control && control.getDisabled && control.setDisabled) {
                        control.setDisabled(true);
                    }
                } catch (e) {
                    console.warn(`Error locking control ${control.getName()}:`, e);
                }
            });

            // Show banner message
            formContext.ui.setFormNotification(
                "This record is locked due to its status and cannot be edited.",
                "INFO",
                "lock_notice"
            );
        } else {
            // Remove notification if status is editable
            formContext.ui.clearFormNotification("lock_notice");
        }

    } catch (error) {
        console.error("Error in onFormLoad:", error);
    }
}




function setDurationBasedOnEntryType(executionContext) {
    try {
        var formContext = executionContext.getFormContext();

        // Get attributes
        var entryTypeAttr = formContext.getAttribute("new_entrytype");
        var isHolidayAttr = formContext.getAttribute("bcc_isholiday");
        var durationAttr = formContext.getAttribute("msdyn_duration"); // minutes
        var totaldurationAttr = formContext.getAttribute("bcc_total_hours"); // hours

        if (!entryTypeAttr || !isHolidayAttr || !durationAttr || !totaldurationAttr) return;

        var entryType = entryTypeAttr.getValue(); // Present/Absent
        var isHoliday = isHolidayAttr.getValue(); // Yes/No boolean

        var durationMinutes = 0;  // for msdyn_duration
        var durationHours = 0;    // for bcc_total_hours

        // Entry Type = Present
        if (entryType === 100000000) {
            if (isHoliday === true || isHoliday === 1) {
                // Present + Holiday = Yes
                durationMinutes = 0;
                durationHours = 0;
            } else {
                // Present + Holiday = No
                durationMinutes = 480; // 8 hours
                durationHours = 8;     // hours
            }
        } 
        // Any other entry type (Absent/Null)
        else {
            durationMinutes = 0;
            durationHours = 0;
        }

        // Set msdyn_duration (MINUTES)
        durationAttr.setValue(durationMinutes);
        durationAttr.setSubmitMode("always");

        // Set Total Hours (HOURS)
        totaldurationAttr.setValue(durationHours);
        totaldurationAttr.setSubmitMode("always");

    } catch (e) {
        console.log("Error in setDurationBasedOnEntryType: " + e.message);
    }
}

function onfFormLoad(executionContext) {
    var formContext = executionContext.getFormContext();

    // Always hide Absent Reason on form load
    var absentReasonCtrl = formContext.getControl("bcc_absentreason");
    if (absentReasonCtrl) absentReasonCtrl.setVisible(false);
}





function absentreason_OnChange(executionContext) {
    var formContext = executionContext.getFormContext();

    // Field names
    var reasonField = "bcc_absentreason";
    var uploadField = "bcc_upload";
    var entryTypeField = "new_entrytype";

    // Option values
    var sickLeaveValue = 755700000;
    var presentValue = 100000000;

    // Get attributes and control
    var reasonAttr = formContext.getAttribute(reasonField);
    var entryTypeAttr = formContext.getAttribute(entryTypeField);
    var uploadAttr = formContext.getAttribute(uploadField);
    var uploadCtrl = formContext.getControl(uploadField);

    if (!reasonAttr || !entryTypeAttr || !uploadAttr || !uploadCtrl) return;

    var absentReason = reasonAttr.getValue();
    var entryType = entryTypeAttr.getValue();

    // Logic
    if (absentReason === sickLeaveValue && entryType !== presentValue) {
        uploadCtrl.setVisible(true);
        uploadAttr.setRequiredLevel("required");   
    } else {
        uploadCtrl.setVisible(false);
        uploadAttr.setRequiredLevel("none");      
    }
}






function onFormmLoad(executionContext) {
    var formContext = executionContext.getFormContext();
    
    // Get the form type
    // 1 = Create, 2 = Update
    var formType = formContext.ui.getFormType();
    
    // If form is in Update mode (record already exists)
    if (formType === 2) {
        // Lock the date field
        var dateField = formContext.getAttribute("new_date");
        if (dateField !== null) {
            var dateControl = formContext.getControl("new_date");
            if (dateControl !== null) {
                dateControl.setDisabled(true);
            }
        }
        
        // Lock the bookable resource field
        var resourceField = formContext.getAttribute("msdyn_bookableresource");
        if (resourceField !== null) {
            var resourceControl = formContext.getControl("msdyn_bookableresource");
            if (resourceControl !== null) {
                resourceControl.setDisabled(true);
            }
        }
    }
}





