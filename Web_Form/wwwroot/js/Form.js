document.addEventListener("DOMContentLoaded", function () {
    const addButton = document.getElementById("AddSection");
    const questionContainer = document.getElementById("questionContainer");
    const questionTemplate = document.getElementById("questionTemplate");

    let questionCount = 0;


    addButton.addEventListener("click", function () {
        questionCount++;


        const newQuestionSection = document.importNode(questionTemplate.content, true);


        const questionLabel = newQuestionSection.querySelector('.question-number');
        questionLabel.textContent = `Question ${questionCount}`;


        questionContainer.appendChild(newQuestionSection);


        attachDeleteEvents();
    });

    function attachDeleteEvents() {
        document.querySelectorAll('.btn-delete').forEach(button => {
            button.onclick = function () {
                button.closest('.question-section').remove();
                updateQuestionNumbers();
            };
        });
    }


    function updateQuestionNumbers() {
        questionCount = 0;
        document.querySelectorAll('.question-section').forEach(section => {
            const questionLabel = section.querySelector('.question-number');
            questionLabel.textContent = `Question ${++questionCount}`;
        });
    }
});