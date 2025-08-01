﻿/**
 * Handles mechanic selection and date picker logic for the appointment edit form.
 * Fetches available mechanics dynamically based on the selected date.
 *
 * Dependencies: jQuery, flatpickr
 */
$(document).ready(function () {
    const appointmentDateInput = $('#AppointmentDate');
    const mechanicSelect = $('#MechanicId');
    const mechanicSpinner = $('#mechanic-spinner');

    // Initialize the date picker
    flatpickr("#AppointmentDate", {
        enableTime: true,
        dateFormat: "Y-m-d H:i",
        minDate: "today",
        time_24hr: true,
        minuteIncrement: 30
        // You can add back the 'disable' logic for full days if you wish
    });

    /**
     * Fetches available mechanics for the selected appointment date and updates the dropdown.
     * Attempts to re-select the original mechanic if still available.
     */
    function fetchAvailableMechanics() {
        const appointmentDate = appointmentDateInput.val();
        const originalMechanicId = mechanicSelect.data('original-mechanic-id'); // Store the initial ID

        mechanicSelect.empty().append('<option value="">Loading...</option>').prop('disabled', true);
        mechanicSpinner.show();

        if (!appointmentDate) {
            mechanicSpinner.hide();
            mechanicSelect.empty().append('<option value="">Please select a date and time</option>');
            return;
        }

        $.getJSON(`/Appointment/GetAvailableMechanics?appointmentDate=${appointmentDate}`)
            .done(function (data) {
                mechanicSelect.empty();
                mechanicSelect.append($('<option>', { value: '', text: 'Select an available mechanic...' }));

                if (data && data.length > 0) {
                    mechanicSelect.prop('disabled', false);
                    $.each(data, function (index, item) {
                        mechanicSelect.append($('<option>', { value: item.value, text: item.text }));
                    });

                    // Try to re-select the original mechanic if they are in the new list
                    if (data.some(m => m.value === originalMechanicId)) {
                        mechanicSelect.val(originalMechanicId);
                    }
                } else {
                    mechanicSelect.append('<option value="">No mechanics available at this time</option>');
                }
            })
            .fail(function () {
                mechanicSelect.empty().append('<option value="">Error loading mechanics</option>');
            })
            .always(function () {
                mechanicSpinner.hide();
            });
    }

    // --- Event Handler ---
    /**
     * Event handler: fetch mechanics when the appointment date changes.
     */
    appointmentDateInput.on('change', fetchAvailableMechanics);

    // --- Initial State ---
    // Store the initial mechanic ID on page load
    // Store the initial mechanic ID on page load and fetch mechanics for the initial date
    mechanicSelect.data('original-mechanic-id', mechanicSelect.val());
    fetchAvailableMechanics();
});