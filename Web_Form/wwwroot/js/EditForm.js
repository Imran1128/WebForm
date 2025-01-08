window.onbeforeunload = function () {
    $.ajax({
        async: false, // Ensure request completes before page unloads
        cache: false,
        url: '/Forms/ClearSession', // Endpoint to clear the server-side session
        type: 'POST',
        success: function () {
            console.log("Server-side session cleared.");
        },
        error: function (xhr, status, error) {
            console.error("Error clearing server-side session: " + error);
        }
    });
};
$(document).ready(function () {
    // Initially hide the Option element
    $("#Option").hide();

    // Function to show/hide Option based on the QuestionType value
    function showOptionAddButton() {
        if ($('#TblQuestion_QuestionType').val() == 3) {
            $("#Option").show();
        } else {
            $("#Option").hide();
        }
    }

    // Call the function when the page loads to check the initial value of QuestionType
    showOptionAddButton();

    // Call the function when the value of QuestionType changes
    $('#TblQuestion_QuestionType').on('change', function () {
        showOptionAddButton();
    });
});

$(document).ready(function () {
    var tagify = new Tagify(document.getElementById('tagInput'));  // Initialize Tagify

    $("#Submit").on("click", function (e) {
        e.preventDefault(); // Prevent default form submission
        $("#Submit").prop('disabled', true); // Disable the submit button to prevent multiple clicks

        // Get clean values from Tagify


        $.ajax({
            async: true,
            cache: false,
            url: '/Forms/Create',
            type: 'POST',
            data: $("#mainForm").serialize(), // Append tag values to form data
            success: function (response) {
                console.log("Form submitted successfully");
                // Optionally redirect or display a success message
                window.location.href = '/Forms/Index';  // Redirect after success
            },
            error: function (xhr, status, error) {
                console.error("Error: " + error);
                $("#Submit").prop('disabled', false); // Re-enable button on error
            }
        });
    });
});


function ResetAddOption() {
    $.ajax({
        async: true,
        cache: false,
        url: '/Forms/AddOptionReset',
        type: 'POST',
        data: $("#mainForm").serialize(),
        success: function (response) {
            // console.log(response);
            $('#optionList').html(response);
        },
        error: function (xhr, status, error) {
            alert("An error occurred: " + xhr.responseText);
            console.error(error);
        }
    });
}
$(document).ready(function () {
    ResetAddOption();
    $("#AddSection").on("click", function () {
        console.log($("#mainForm").serialize());
        $.ajax({
            async: true,
            cache: false,
            url: '/Forms/AddQuestionEdit',
            type: 'POST',
            data: $("#mainForm").serialize(),
            success: function (response) {
                // console.log(response);
                $('#questionDetails').html(response);
                ResetAddOption();
            },
            error: function (xhr, status, error) {
                alert("An error occurred: " + xhr.responseText);
                console.error(error);
            }
        });
    });
    $("#AddOption").on("click", function () {
        $.ajax({
            async: true,
            cache: false,
            url: '/Forms/AddOption',
            type: 'POST',
            data: $("#mainForm").serialize(),
            success: function (response) {
                // console.log(response);
                $('#optionList').html(response);
            },
            error: function (xhr, status, error) {
                alert("An error occurred: " + xhr.responseText);
                console.error(error);
            }
        });
    });
});
$(document).ready(function () {
    // Monitor changes to the visibility dropdown
    $('#privateDetails').hide();
    $('#visibility').on('change', function () {
        const selectedValue = $(this).val();

        if (selectedValue === 'false') {
            // Show and enable the private details field
            $('#privateDetails').show(); // Make the div visible
            // Make the input visible
        } else {
            // Hide the private details section
            $('#privateDetails').hide()
        }
    });
});

$(document).ready(function () {
    let selectedTag = null;

    // Listen for input events on the tag input field
    $('#tagInput').on('input', function () {
        const query = this.value.trim();
        if (query.length > 0) {
            fetchTags(query);
        } else {
            clearSuggestions();
        }
    });

    // Fetch tags from the API endpoint
    function fetchTags(query) {
        fetch(`/forms/AutoCompleteTagApi?query=${query}`)
            .then(response => response.json())
            .then(data => {
                displaySuggestions(data, query);
            })
            .catch(error => console.error('Error fetching autocomplete data:', error));
    }

    // Display matching tags in the suggestions container
    function displaySuggestions(forms, query) {
        const suggestionsContainer = $('#autocomplete-suggestions');
        suggestionsContainer.empty(); // Clear existing suggestions

        // Filter forms to match the query
        const matches = forms.filter(form => form.tag && form.tag.toLowerCase().includes(query.toLowerCase()));

        if (matches.length > 0) {
            matches.forEach(form => {
                const suggestionItem = $('<div>')
                    .addClass('suggestion-item')
                    .text(form.tag);
                suggestionItem.on('click', function () {
                    selectedTag = form.tag; // Store selected tag
                    $('#tagInput').val(form.tag); // Set input field to tag
                    clearSuggestions();
                });
                suggestionsContainer.append(suggestionItem);
            });
        } else {
            suggestionsContainer.html('<div class="suggestion-item">No results found</div>');
        }
    }

    // Clear suggestions container
    function clearSuggestions() {
        $('#autocomplete-suggestions').empty();
    }

    // Event listener for Add Button
    $('#add-button').on('click', function () {
        if (selectedTag) {
            // POST the selected tag to the backend
            postSelectedTag(selectedTag);
        } else {
            alert("Please select a tag first.");
        }
    });

    // Function to POST selected tag data to the backend
    function postSelectedTag(tag) {
        fetch('https://localhost:7078/forms/SubmitSelectedTag', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ tag: tag })
        })
            .then(response => response.json())
            .then(data => {
                console.log('Tag posted successfully:', data);
                displaySelectedTag(tag);
            })
            .catch(error => console.error('Error posting tag:', error));
    }

    // Function to display the selected tag below the input field
    function displaySelectedTag(tag) {
        const selectedTagContainer = $('#selected-tag');
        selectedTagContainer.text(`Selected Tag: ${tag}`);
    }
});

$(document).ready(function () {

    $('.users-multiple').select2();
});

$(document).ready(function () {
    // Fetch whitelist from the database via API
    $.ajax({
        url: '/forms/AutoCompleteTagApi',  // Replace with your actual API endpoint
        method: 'GET',
        success: function (response) {
            console.log('API response:', response);  // Log response for debugging

            // Ensure response is an array of strings (this should be true based on the example you provided)
            if (Array.isArray(response)) {
                var input = document.querySelector('#tagsInput');
                var tagify = new Tagify(input, {
                    whitelist: response,  // Use the fetched whitelist
                    enforceWhitelist: false,
                    dropdown: {
                        enabled: 1,
                        maxItems: 10
                    },
                    addTagOnBlur: true,
                    maxTags: 10,
                })
                tagify.getCleanValue();
            } else {
                console.error('Invalid whitelist format received from the API');
            }
        },
        error: function (error) {
            console.error('Error fetching whitelist:', error);
        }
    });

    // // Handle the 'change' event to extract a list of strings from the tags
    //     tagify.on('change', function (e) {
    //         var tagValues = tagify.getCleanValue();  // e.g., [{ "value": "d" }, { "value": "df" }]

    //         // Map to extract only the 'value' property of each tag
    //         var tagsArray = tagValues.map(tag => tag.value);  // e.g., ["d", "df"]

    //         // Set the value of the hidden input field 'tagsData' to the tagsArray as JSON string
    //         $('#tagsData').val(JSON.stringify(tagsArray));  // Add tag values as a JSON string
    //         console.log(tagsArray)          // Prints the list of tags (strings)

    //         // Example: Sending the tags as a list of strings to the server
    //         sendTags(tagValues);
    //     });


    //     function sendTags(tagValues) {
    //         fetch('/YourController/YourAction', {
    //             method: 'POST',
    //             headers: {
    //                 'Content-Type': 'application/json'
    //             },
    //             body: JSON.stringify({ tags: tagsArray })
    //         })
    //             .then(response => response.json())
    //             .then(data => console.log('Tags sent:', data))
    //             .catch(error => console.error('Error:', error));
    //     }
});