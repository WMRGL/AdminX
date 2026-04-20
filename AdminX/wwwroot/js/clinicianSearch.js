let _clinicianDropdownTargetId = 'referrerCode';

function SetClinicianTarget(targetId) {
    _clinicianDropdownTargetId = targetId;
}

async function SearchClinicians() {
    const firstName = document.getElementById('inlineFormInputName2').value;
    const lastName = document.getElementById('inlineFormInputName1').value;
    const isOnlyCurrent = document.getElementById('chkOnlyCurrent').checked;
    const isOnlyGP = document.getElementById('chkOnlyGP').checked;
    const isOnlyNonGP = document.getElementById('chkOnlyNonGP').checked;

    const url = `/Referral/SearchExternalClinicians?firstName=${encodeURIComponent(firstName)}&lastName=${encodeURIComponent(lastName)}&isOnlyCurrent=${isOnlyCurrent}&isOnlyGP=${isOnlyGP}&isOnlyNonGP=${isOnlyNonGP}`;

    try {
        const response = await fetch(url);
        const data = await response.json();

        const tbody = document.getElementById('tblClinicianResultsBody');
        tbody.innerHTML = '';

        if (data && data.length > 0) {
            data.forEach(item => {
                const fullName = `${item.title || ''} ${item.firstName || ''} ${item.lastName || ''}`.trim();
                const tr = document.createElement('tr');

                tr.innerHTML = `
                    <td>${item.masterClinicianCode}</td>
                    <td>${fullName}</td>
                    <td>${item.facility || ''}</td>
                    <td>${item.position || ''}</td>
                    <td>
                        <button type="button" class="btn btn-sm btn-primary"
                            onclick="SelectExternalClinician('${item.masterClinicianCode}', '${fullName.replace(/'/g, "\\'")}', '${(item.facility || '').replace(/'/g, "\\'")}')">
                            Select
                        </button>
                    </td>
                `;
                tbody.appendChild(tr);
            });
            document.getElementById('tblClinicianResults').style.display = 'table';
        } else {
            tbody.innerHTML = '<tr><td colspan="5" class="text-center">No clinicians found matching those criteria.</td></tr>';
            document.getElementById('tblClinicianResults').style.display = 'table';
        }
    } catch (error) {
        console.error('Error searching for clinicians:', error);
        alert('An error occurred whilst searching. Please try again.');
    }
}

function SelectExternalClinician(code, name, facility) {
    var referrerList = document.getElementById(_clinicianDropdownTargetId);
    var displayText = `${code} - ${name}, ${facility}`;

    if (referrerList && referrerList.choices) {
        referrerList.choices.setChoices([
            { value: code, label: displayText, selected: true }
        ], 'value', 'label', false);
    } else if (referrerList) {
        var optionExists = Array.from(referrerList.options).some(opt => opt.value === code);
        if (!optionExists) {
            var newOption = document.createElement("option");
            newOption.value = code;
            newOption.text = displayText;
            referrerList.add(newOption);
        }
        referrerList.value = code;
    }

    var modalEl = document.getElementById('centeredModalSuccess');
    var modalInstance = bootstrap.Modal.getInstance(modalEl);
    if (modalInstance) {
        modalInstance.hide();
    }
}

function selectGen() {
    // 1. Use the dynamic target ID so it works on ALL pages
    var referrerList = document.getElementById(_clinicianDropdownTargetId);

    if (!referrerList) {
        console.error("Dropdown element not found: " + _clinicianDropdownTargetId);
        return;
    }

    const newCode = "GEN";

    // 2. Set the value directly without doing manual loops
    if (referrerList.choices) {
        referrerList.choices.setChoiceByValue(newCode);
    } else {
        referrerList.value = newCode;
    }

    // 3. Force the facility update
    if (typeof updateFacility === "function") {
        updateFacility();
    }
}

function SetToSelf() {
    // 1. Use the dynamic target ID instead of hardcoding "ddlRefPhys"
    var referrerList = document.getElementById(_clinicianDropdownTargetId);

    if (!referrerList) {
        console.error("Dropdown element not found: " + _clinicianDropdownTargetId);
        return;
    }

    const newCode = "Self/FamMemb";

    // 2. Set the value directly
    if (referrerList.choices) {
        referrerList.choices.setChoiceByValue(newCode);
    } else {
        referrerList.value = newCode;
    }

    // 3. Force the facility update
    if (typeof updateFacility === "function") {
        updateFacility();
    }
}


function ToggleClinicianViews(view) {
    if (view === 'add') {
        document.getElementById('clinicianSearchBlock').style.display = 'none';
        document.getElementById('clinicianAddBlock').style.display = 'block';

        document.getElementById('addFacilitySearch').value = '';
        document.getElementById('addFacility').innerHTML = '<option value="">Please type above to search...</option>';

        LoadTitles();
    } else {
        document.getElementById('clinicianSearchBlock').style.display = 'block';
        document.getElementById('clinicianAddBlock').style.display = 'none';
    }
}

let _facilitySearchTimer;

function DebounceFacilitySearch() {
    clearTimeout(_facilitySearchTimer);
    _facilitySearchTimer = setTimeout(LoadFacilities, 400);
}

async function LoadFacilities() {
    const searchTerm = document.getElementById('addFacilitySearch').value;
    const facSelect = document.getElementById('addFacility');

    if (searchTerm.trim().length < 2) {
        facSelect.innerHTML = '<option value="">Please type at least 2 characters to search...</option>';
        return;
    }

    facSelect.innerHTML = '<option value="">Searching...</option>';
    facSelect.disabled = true;

    try {
        const response = await fetch(`/SysAdmin/GetFacilitiesAjax?searchTerm=${encodeURIComponent(searchTerm)}`);
        const data = await response.json();

        if (data.length === 0) {
            facSelect.innerHTML = '<option disabled value="">No facilities found matching that search.</option>';
            return;
        }

        facSelect.innerHTML = data.map(fac => `<option value="${fac.code}">${fac.name} - ${fac.code}</option>`).join('');

    } catch (error) {
        console.error('Error searching facilities:', error);
        facSelect.innerHTML = '<option value="">Error searching facilities</option>';
    } finally {
        facSelect.disabled = false;
    }
}

async function SubmitNewClinician() {
    const btn = document.getElementById('btnSaveNewClinician');
    btn.disabled = true;
    btn.innerText = "Saving...";

    const formData = new FormData();
    formData.append('clinCode', document.getElementById('addClinCode').value);
    formData.append('title', document.getElementById('addTitle').value);
    formData.append('firstName', document.getElementById('addFirstName').value);
    formData.append('lastName', document.getElementById('addLastName').value);
    formData.append('speciality', document.getElementById('addSpeciality').value);
    formData.append('jobTitle', document.getElementById('addPosition').value);
    formData.append('isGP', document.getElementById('addIsGP').value);
    formData.append('facilityCode', document.getElementById('addFacility').value);

    try {
        const response = await fetch('/SysAdmin/AddNewClinicianAjax', {
            method: 'POST',
            body: formData
        });
        const data = await response.json();

        if (data.success) {
            SelectExternalClinician(data.masterClinicianCode, `${data.title} ${data.firstName} ${data.lastName}`, data.facility);

            document.getElementById('frmAddClinician').reset();
            ToggleClinicianViews('search');
        } else {
            alert("Failed to save: " + data.message);
        }
    } catch (error) {
        console.error('Error saving clinician:', error);
        alert("An error occurred while saving. Please try again.");
    } finally {
        btn.disabled = false;
        btn.innerText = "Save & Select Clinician";


    }
}




async function LoadTitles() {
    const titleSelect = document.getElementById('addTitle');

    if (titleSelect.options.length > 1) return;

    try {
        const response = await fetch('/SysAdmin/GetTitlesAjax');
        const data = await response.json();

        let optionsHtml = '<option value="">Select...</option>';
        optionsHtml += data.map(t => `<option value="${t.title}">${t.title}</option>`).join('');

        titleSelect.innerHTML = optionsHtml;
    } catch (error) {
        console.error('Error loading titles:', error);
        titleSelect.innerHTML = '<option value="">Error</option>';
    }
}




