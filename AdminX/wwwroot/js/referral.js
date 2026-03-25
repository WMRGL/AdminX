function ShowCancerStuff() {
    var cStuff = document.getElementById("cancerStuff");

    // var adminStatusDropdown = document.getElementById("admin_status");

    if (document.getElementById("refPathway").value == "Cancer") {
        cStuff.hidden = false;
        // adminStatusDropdown.required = true;
        document.getElementById("type").value = "New Referral";
        document.getElementById("ddlIndication").value = "?Cancer";
        document.getElementById("ddlClinClass").value = "Routine";
        document.getElementById("ddlPregnancy").value = "No Pregnancy";
        document.getElementById("admin_status").value = "New Referral";

        document.getElementById("ddlConsultant").value = "@Model.areaName.ConsCode.ToUpper()";
        document.getElementById("ddlGc").value = "@Model.areaName.GC.ToUpper()";
        document.getElementById("ddlAdmin").value = "@Model.areaName.MedSecCode.ToUpper()";
    }
    else {
        cStuff.hidden = true;
        // adminStatusDropdown.required = false;
        document.getElementById("type").value = "Incomplete Referral";
        document.getElementById("ddlIndication").value = "?Other";
        document.getElementById("ddlClinClass").value = "";
        document.getElementById("ddlPregnancy").value = "";
        document.getElementById("admin_status").value = "";

        document.getElementById("ddlConsultant").value = "";
        document.getElementById("ddlGc").value = "";
        document.getElementById("ddlAdmin").value = "";
    }
}

function SetReferrerToGP() {
    var _refer = document.getElementById("ddlRefPhys");
    const _newRefer = "@Model.patient.GP_Code";

    if (_refer.choices) {
        _refer.choices.setChoiceByValue(_newRefer);
    } else {
        _refer.value = _newRefer;
    }
}