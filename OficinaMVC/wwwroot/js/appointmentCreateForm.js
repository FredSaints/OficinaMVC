/**
 * Handles appointment creation form logic: dynamic dropdowns, date picker, and mechanic availability.
 * Integrates with flatpickr for date selection and uses AJAX for dynamic data.
 *
 * Dependencies: jQuery, flatpickr
 */
$(document).ready(function () {
    // --- Get all the form elements ---
    const clientSelect = $('#ClientId');
    const vehicleSelect = $('#VehicleId');
    const appointmentDateInput = $('#AppointmentDate');
    const mechanicSelect = $('#MechanicId');
    const submitButton = $('form button[type="submit"]');
    const mechanicSpinner = $('#mechanic-spinner');
    let unavailableDates = [];

    // --- Date Picker Logic ---
    /**
     * Fetches unavailable appointment days for the given month/year and updates the unavailableDates array.
     * @param {number} year - The year to fetch unavailable days for.
     * @param {number} month - The month (0-based) to fetch unavailable days for.
     * @param {function} [callback] - Optional callback to run after data is loaded.
     */
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

    // --- AJAX Functions ---
    /**
     * Fetches available mechanics for the selected appointment date and updates the dropdown.
     */
    function fetchAvailableMechanics() {
        const appointmentDate = appointmentDateInput.val();
        mechanicSelect.empty().append('<option value="">Loading...</option>').prop('disabled', true);
        mechanicSpinner.show();

        if (appointmentDate) {
            $.getJSON(`/Appointment/GetAvailableMechanics?appointmentDate=${appointmentDate}`)
                .done(function (data) {
                    mechanicSelect.empty();
                    if (data && data.length > 0) {
                        mechanicSelect.prop('disabled', false);
                        $.each(data, function (index, item) {
                            mechanicSelect.append($('<option>', { value: item.value, text: item.text }));
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
                });
        }
    }

    // --- Event Handlers ---
    /**
     * Event handler: fetch mechanics when the appointment date changes.
     */
    appointmentDateInput.on('change', fetchAvailableMechanics);

    /**
     * Event handler: fetch vehicles when the client changes, and disable submit until valid.
     */
    clientSelect.on('change', function () {
        const clientId = $(this).val();
        vehicleSelect.empty().append('<option value="">Loading...</option>').prop('disabled', true);
        submitButton.prop('disabled', true);

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

    // Enable button only when a valid vehicle is selected
    /**
     * Event handler: enable submit button only when a valid vehicle is selected.
     */
    vehicleSelect.on('change', function () {
        if ($(this).val()) {
            submitButton.prop('disabled', false);
        } else {
            submitButton.prop('disabled', true);
        }
    });

    // --- Initial State ---
    submitButton.prop('disabled', true);
});