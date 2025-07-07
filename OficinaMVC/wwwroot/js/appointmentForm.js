$(document).ready(function () {
    // --- Get all the form elements ---
    const clientSelect = $('#ClientId');
    const vehicleSelect = $('#VehicleId');
    const appointmentDateInput = $('#AppointmentDate');
    const mechanicSelect = $('#MechanicId');
    const serviceTypeSelect = $('#ServiceTypeId'); // Get the service type dropdown
    const submitButton = $('form button[type="submit"]');
    const mechanicSpinner = $('#mechanic-spinner');
    let unavailableDates = [];

    // --- Core Logic: Function to check if the form is valid to be submitted ---
    function checkFormValidity() {
        // This function determines if the submit button should be enabled.
        const isCreatePage = clientSelect.length > 0;

        // Common conditions for both pages
        const dateSelected = appointmentDateInput.val() !== '';
        const serviceSelected = serviceTypeSelect.val() !== '' && serviceTypeSelect.val() !== '0';
        const mechanicSelected = mechanicSelect.val() !== '' && !mechanicSelect.prop('disabled');

        let isFormValid = false;

        if (isCreatePage) {
            // On Create page, we also need a client and vehicle
            const clientSelected = clientSelect.val() !== '';
            const vehicleSelected = vehicleSelect.val() !== '' && !vehicleSelect.prop('disabled');
            isFormValid = clientSelected && vehicleSelected && dateSelected && serviceSelected && mechanicSelected;
        } else {
            // On Edit page, client and vehicle are static, so we only check the others
            isFormValid = dateSelected && serviceSelected && mechanicSelected;
        }

        // Enable or disable the button based on the result
        submitButton.prop('disabled', !isFormValid);
    }

    // --- Date Picker Logic (flatpickr) ---
    function fetchUnavailableDays(year, month, callback) {
        $.getJSON(`/Appointment/GetUnavailableDays?year=${year}&month=${month + 1}`, function (data) {
            unavailableDates = data;
            if (callback) callback();
        });
    }

    flatpickr("#AppointmentDate", {
        enableTime: true,
        dateFormat: "Y-m-d H:i",
        minDate: "today",
        time_24hr: true,
        minuteIncrement: 30,
        disable: [
            function (date) {
                if (date.getDay() === 0 || date.getDay() === 6) return true;
                const dateString = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
                return unavailableDates.includes(dateString);
            }
        ],
        onMonthChange: function (s, d, instance) { fetchUnavailableDays(instance.currentYear, instance.currentMonth, () => instance.redraw()); },
        onOpen: function (s, d, instance) { if (unavailableDates.length === 0) fetchUnavailableDays(instance.currentYear, instance.currentMonth, () => instance.redraw()); }
    });

    // --- AJAX Function to fetch mechanics ---
    function fetchAvailableMechanics() {
        const appointmentDate = appointmentDateInput.val();
        mechanicSelect.empty().append('<option value="">Loading...</option>').prop('disabled', true);
        mechanicSpinner.show();

        $.getJSON(`/Appointment/GetAvailableMechanics?appointmentDate=${appointmentDate}`)
            .done(function (data) {
                mechanicSelect.empty();
                if (data && data.length > 0) {
                    mechanicSelect.prop('disabled', false);
                    $.each(data, function (index, item) {
                        const isSelected = item.value === mechanicSelect.data('current-mechanic-id');
                        mechanicSelect.append($('<option>', { value: item.value, text: item.text, selected: isSelected }));
                    });
                } else {
                    mechanicSelect.append('<option value="">No mechanics available</option>');
                }
            })
            .fail(function () {
                mechanicSelect.empty().append('<option value="">Error loading mechanics</option>');
            })
            .always(function () {
                mechanicSpinner.hide();
                // After the call is complete, always check the form's validity
                checkFormValidity();
            });
    }

    // --- Event Handlers ---

    // Any time a key dropdown changes, check the form validity
    serviceTypeSelect.on('change', checkFormValidity);
    mechanicSelect.on('change', checkFormValidity);

    // The date input needs to fetch mechanics AND check validity
    appointmentDateInput.on('change', function () {
        fetchAvailableMechanics();
        checkFormValidity();
    });

    // Event handlers specific to the Create page
    if (clientSelect.length) {
        clientSelect.on('change', function () {
            const clientId = $(this).val();
            vehicleSelect.empty().append('<option value="">Loading...</option>').prop('disabled', true);
            checkFormValidity(); // Check validity to disable button

            if (clientId) {
                $.getJSON(`/Appointment/GetVehiclesByClient?clientId=${clientId}`)
                    .done(function (data) {
                        vehicleSelect.empty().append('<option value="">Select a vehicle...</option>');
                        if (data && data.length > 0) {
                            vehicleSelect.prop('disabled', false);
                            $.each(data, function (index, item) {
                                vehicleSelect.append($('<option>', { value: item.value, text: item.text }));
                            });
                        } else {
                            vehicleSelect.empty().append('<option value="">No vehicles found</option>');
                        }
                    });
            } else {
                vehicleSelect.empty().append('<option value="">Select a client first</option>');
            }
        });

        vehicleSelect.on('change', checkFormValidity);
    }

    // --- Initial Page Load Setup ---
    if (!clientSelect.length) { // We are on the EDIT page
        // On the edit page, we need to fetch mechanics for the initial date
        const currentMechanicId = mechanicSelect.val();
        mechanicSelect.data('current-mechanic-id', currentMechanicId);
        fetchAvailableMechanics();
    }

    // Run one final check on page load for all scenarios.
    checkFormValidity();
});