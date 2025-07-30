/**
 * Handles dynamic population of the vehicle model dropdown based on the selected brand.
 * Fetches car models via AJAX and updates the model dropdown accordingly.
 * Also restores the current model selection if editing an existing vehicle.
 *
 * Dependencies: jQuery
 */
$(document).ready(function () {
    /**
     * Brand dropdown element.
     * @type {JQuery<HTMLElement>}
     */
    const brandSelect = $('#BrandId');
    /**
     * Model dropdown element.
     * @type {JQuery<HTMLElement>}
     */
    const modelSelect = $('#CarModelId');
    /**
     * Spinner element shown while loading models.
     * @type {JQuery<HTMLElement>}
     */
    const modelSpinner = $('#model-spinner');

    /**
     * Event handler for when the brand selection changes.
     * Fetches car models for the selected brand and updates the model dropdown.
     */
    brandSelect.on('change', function () {
        const brandId = $(this).val();

        modelSelect.empty();
        modelSelect.prop('disabled', true);

        if (brandId && brandId !== "0") {
            modelSpinner.show();

            /**
             * AJAX request to fetch car models for the selected brand.
             */
            $.ajax({
                url: `/Vehicle/GetCarModels?brandId=${brandId}`,
                type: 'GET',
                success: function (data) {
                    modelSelect.prop('disabled', false);
                    modelSelect.append('<option value="">Select a model...</option>');

                    /**
                     * Populate the model dropdown with the fetched car models.
                     */
                    $.each(data, function (index, carModel) {
                        if (carModel.value !== "0") {
                            modelSelect.append(
                                $('<option>', {
                                    value: carModel.value,
                                    text: carModel.text
                                })
                            );
                        }
                    });

                    // Restore the current model selection if editing
                    const currentModelId = modelSelect.data('current-model-id');
                    if (currentModelId) {
                        modelSelect.val(currentModelId);
                    }
                },
                error: function (error) {
                    console.error("Error fetching car models: ", error);
                    modelSelect.prop('disabled', true);
                    modelSelect.append('<option value="">Error loading models</option>');
                },
                complete: function () {
                    modelSpinner.hide();
                }
            });
        } else {
            modelSelect.append('<option value="">Select a brand first</option>');
        }
    });

    // If a brand is already selected (edit mode), trigger the change event to load models
    if (brandSelect.val() && brandSelect.val() !== "0") {
        brandSelect.trigger('change');
    }
});