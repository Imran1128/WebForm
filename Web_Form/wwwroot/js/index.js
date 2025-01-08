$(document).ready(function () {
    $.ajax({
        url: '/Forms/LatestTemplateApi',
        type: 'GET',
        success: function (data) {
            $('#latest-forms').empty();
            data.slice(0, 3).forEach(function (item) {
                var likeLabel = item.likes === 1 ? 'Like' : 'Likes';
                var cardHtml = `
                                            <div class="col-lg-4 col-md-6 mb-4">
            <div class="card shadow-lg border-0 rounded-4 overflow-hidden h-100">
                <div class="card-body p-4 d-flex flex-column bg-light rounded-3">
                    <a href="/Forms/Details?FormId=${item.formId}" class="text-decoration-none text-dark">
                        <div class="d-flex align-items-center mb-3">
                            <i class="bi bi-file-earmark-text-fill text-primary fs-4 me-2"></i>
                            <h5 class="card-title text-primary fs-4 mb-0" style="font-weight: 600;">${item.title}</h5>
                        </div>
                        <p class="card-text text-muted small mb-3">
                            <strong><i class="bi bi-calendar-event me-2"></i> Created On:</strong> ${item.createdOn}
                        </p>
                        <p class="card-text text-muted small mb-3">
                            <strong><i class="bi bi-person-circle me-2"></i> Author:</strong> ${item.createdby}
                        </p>
                        <div class="d-flex justify-content-between align-items-center">
                            <span class="badge bg-primary text-white rounded-pill px-3 py-2">
                                <i class="bi bi-pencil-fill me-1"></i> Fill Up
                            </span>
                            <span class="badge bg-danger text-white rounded-pill px-3 py-2">
                                <i class="bi bi-heart me-1"></i> ${item.likes} ${likeLabel}
                            </span>
                        </div>
                    </a>
                </div>
            </div>
        </div>

                                `;
                $('#latest-forms').append(cardHtml);
            });
        },
        error: function (xhr, status, error) {
            console.error("Error fetching Latest Templates:", error);
        }
    });

    $.ajax({
        url: '/Forms/PopularFormApi',
        type: 'GET',
        success: function (data) {
            $('#popular-forms').empty();
            data.slice(0, 3).forEach(function (item) {
                var likeLabel = item.likes === 1 ? 'Like' : 'Likes';
                var cardHtml = `
                                    <div class="col-lg-4 col-md-6 mb-4">
                                        <div class="card shadow-lg border-0 rounded-3 overflow-hidden h-100">
                                            <div class="card-body p-4 d-flex flex-column bg-light rounded-3">
                                                <a href="/Forms/Details?FormId=${item.formId}" class="text-decoration-none">
                                                    <div class="d-flex align-items-center mb-3">
                                                        <i class="bi bi-file-earmark-text-fill text-primary fs-4 me-2"></i>
                                                        <h5 class="card-title text-primary fs-4 mb-0">${item.title}</h5>
                                                    </div>
                                                    <p class="card-text text-muted small mb-3">
                                                        <strong><i class="bi bi-calendar-event me-2"></i> Created On:</strong> ${item.createdOn}
                                                    </p>
                                                    <p class="card-text text-muted small mb-3">
                                                        <strong><i class="bi bi-person-circle me-2"></i> Author:</strong> ${item.createdby}
                                                    </p>
                                                    <div class="d-flex justify-content-between align-items-center">
                                                        <span class="badge bg-primary text-white">
                                                            <i class="bi bi-pencil-fill me-1"></i> Fill Up
                                                        </span>
                                                        <span class="badge bg-danger text-white">
                                                            <i class="bi bi-heart me-1"></i> ${item.likes} ${likeLabel}
                                                        </span>
                                                    </div>
                                                </a>
                                            </div>
                                        </div>
                                    </div>
                                `;
                $('#popular-forms').append(cardHtml);
            });
        },
        error: function (xhr, status, error) {
            console.error("Error fetching Popular Forms:", error);
        }
    });
});


$(document).ready(function () {

    $('.tags').select2();
});
$(document).ready(function () {
    $('#tagSelect').on('change input', function () {
        var selectedTag = $(this).val();
        console.log("Selected Tag(s):", selectedTag);


        if (!selectedTag || selectedTag.length === 0) {
            console.log("No tags selected.");
            $('#formByTag').empty();
        } else {
            $.ajax({
                url: '/forms/GetByTag',
                type: 'GET',
                data: { tags: selectedTag },
                traditional: true,
                success: function (data) {
                    console.log("Data received:", data);
                    $('#formByTag').empty();


                    data.slice(0, 3).forEach(function (item) {
                        var likeLabel = item.likes === 1 ? 'Like' : 'Likes';
                        var cardHtml = `
                                    <div class="col-lg-4 col-md-6 mb-4">
                                        <div class="card shadow-lg border-0 rounded-3 overflow-hidden h-100">
                                            <div class="card-body p-4 d-flex flex-column bg-light rounded-3">
                                                <a href="/Forms/Details?FormId=${item.formId}" class="text-decoration-none">
                                                    <div class="d-flex align-items-center mb-3">
                                                        <i class="bi bi-file-earmark-text-fill text-primary fs-4 me-2"></i>
                                                        <h5 class="card-title text-primary fs-4 mb-0">${item.title}</h5>
                                                    </div>
                                                    <p class="card-text text-muted small mb-3">
                                                        <strong><i class="bi bi-calendar-event me-2"></i> Created On:</strong> ${item.createdOn}
                                                    </p>
                                                    <p class="card-text text-muted small mb-3">
                                                        <strong><i class="bi bi-person-circle me-2"></i> Author:</strong> ${item.createdby}
                                                    </p>
                                                    <div class="d-flex justify-content-between align-items-center">
                                                        <span class="badge bg-primary text-white">
                                                            <i class="bi bi-pencil-fill me-1"></i> Fill Up
                                                        </span>
                                                        <span class="badge bg-danger text-white">
                                                            <i class="bi bi-heart me-1"></i> ${item.likes} ${likeLabel}
                                                        </span>
                                                    </div>
                                                </a>
                                            </div>
                                        </div>
                                    </div>
                                `;
                        $('#formByTag').append(cardHtml);
                    });
                },
                error: function (xhr, status, error) {
                    console.error('AJAX Request Failed');
                    console.error('Status:', status);
                    console.error('Error:', error);
                    console.error('Response:', xhr.responseText);
                }
            });
        }
    });
});