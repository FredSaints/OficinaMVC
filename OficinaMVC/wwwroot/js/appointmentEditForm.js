$(document).ready(function () {
    // --- Get all the form elements ---
    const appointmentDateInput = $('#AppointmentDate');
    const mechanicSelect = $('#MechanicId');
    const mechanicSpinner = $('#mechanic-spinner');
    let unavailableDates = [];

    // --- Date Picker Logic ---
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
        const currentMechanicId = mechanicSelect.val();
        mechanicSelect.empty().append('<option value="">Loading...</option>').prop('disabled', true);
        mechanicSpinner.show();

        if (appointmentDate) {
            $.getJSON(`/Appointment/GetAvailableMechanics?appointmentDate=${appointmentDate}`)
                .done(function (data) {
                    mechanicSelect.empty();

                    if (data && data.length > 0) {
                        mechanicSelect.prop('disabled', false);

                        $.each(data, function (index, item) {
                            mechanicSelect.append($('<option>', {
                                value: item.value,
                                text: item.text,
                                selected: item.value === currentMechanicId
                            }));
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

    // --- Event Handler ---
    appointmentDateInput.on('change', fetchAvailableMechanics);

    // --- Initial State ---
    // On page load for the Edit form, immediately fetch the available mechanics for the pre-filled date.
    fetchAvailableMechanics();
});