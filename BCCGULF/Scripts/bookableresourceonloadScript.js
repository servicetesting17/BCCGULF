function lockFeildBasedOnRole(executionContext) {
    var formContext = executionContext.getFormContext();

    var userRoles = Xrm.Utility.getGlobalContext().userSettings.roles;
    var hasRole = false;
    var isSiteTransferUser = false;
    var isHrBcc = false;

    userRoles.forEach(function (role) {
        var roleName = role.name.toLowerCase();

        if (roleName === "timesheet keeper" || roleName === "project manager") {
            hasRole = true;
        }

        if (roleName === "site transfer") {
            isSiteTransferUser = true;
        }

        if (roleName === "hr_bcc") {
            isHrBcc = true;
        }
    });

    // HR_BCC overrides the lock — fields remain fully editable
    if (isHrBcc) return;

    // If user has Timesheet Keeper or Project Manager → lock all fields
    if (hasRole) {
        lockAllFields(formContext);
    }

    // Exception: Site Transfer role → new_project should be editable
    if (isSiteTransferUser) {
        var projectControl = formContext.getControl("new_project");
        if (projectControl) {
            projectControl.setDisabled(false);
        }
    }
}

function lockAllFields(formContext) {
    // Lock all attributes (fields)
    formContext.data.entity.attributes.forEach(function (attribute) {
        var control = formContext.getControl(attribute.getName());
        if (control) {
            control.setDisabled(true);
        }
    });

    // Lock all tabs + sections
    formContext.ui.tabs.forEach(function (tab) {
        tab.sections.forEach(function (section) {
            section.controls.forEach(function (control) {
                control.setDisabled(true);
            });
        });
    });
}

let oldProjectId = null;

// -------------------- ONLOAD --------------------
function onLoadHideShowFromDate(executionContext) {
    const fc = executionContext.getFormContext();

    fc.getControl("bcc_fromdate")?.setVisible(false);
    fc.getControl("bcc_fromdate")?.setDisabled(true);
    fc.getAttribute("bcc_fromdate")?.setRequiredLevel("none");
}

// -------------------- ONCHANGE PROJECT --------------------
function onProjectChangeForFromDate(executionContext) {
    const fc = executionContext.getFormContext();
    const attr = fc.getAttribute("new_project");
    if (!attr || !attr.getIsDirty()) return;

    const newVal = attr.getValue();
    const newId = newVal ? newVal[0].id : null;

    if (newId !== oldProjectId) {
        fc.getAttribute("bcc_fromdate")?.setValue(null);
        fc.getControl("bcc_fromdate")?.setVisible(true);
        fc.getControl("bcc_fromdate")?.setDisabled(false);
        fc.getAttribute("bcc_fromdate")?.setRequiredLevel("required");
    } else {
        fc.getControl("bcc_fromdate")?.setVisible(false);
        fc.getControl("bcc_fromdate")?.setDisabled(true);
        fc.getAttribute("bcc_fromdate")?.setRequiredLevel("none");
    }
}

// -------------------- ONSAVE - CREATE TRANSACTION --------------------
function onSaveCreateRecordResourceTransaction(executionContext) {
    const fc = executionContext.getFormContext();
    const attr = fc.getAttribute("new_project");    
var id = fc.data.entity.getId().replace(/[{}]/g, "");

    if (!attr || !attr.getIsDirty()) return;

    const proj = attr.getValue();
    const newId = proj ? proj[0].id.replace(/[{}]/g, "").toLowerCase() : null;

    if (!newId || newId === oldProjectId) return;
    
  
    var record = {};
    record.bcc_dateofjoining = fc.getAttribute("new_dateofjoining")?.getValue(); // Date Time
    record.bcc_nfromdate = fc.getAttribute("bcc_fromdate")?.getValue(); // Date Time
    record.bcc_employeeid = fc.getAttribute("bcc_userid")?.getValue(); // Text
    record["bcc_Project@odata.bind"] = "/bcc_projects(" + newId + ")"; // Lookup  new_project
    record.bcc_resourcename = fc.getAttribute("name")?.getValue(); // Text
    record["bcc_BookableResource@odata.bind"] = "/bookableresources(" + id+ ")"; // Lookup

    Xrm.WebApi.createRecord("bcc_resourcetransaction", record).then(
        function success(result) {
            var newIds = result.id;
            console.log(newIds);
            fc.getAttribute("bcc_fromdate")?.setRequiredLevel("none");
            fc.getAttribute("bcc_fromdate")?.setValue(null);
            fc.getControl("bcc_fromdate")?.setVisible(false);
        },
        function (error) {
            console.log(error.message);
            fc.getAttribute("bcc_fromdate")?.setRequiredLevel("required");            
            fc.getControl("bcc_fromdate")?.setVisible(true);
        }
    );  
}