// Added by Angela: JavaScript for Milestone 3 Minesweeper AJAX updates.

$(document).ready(function () {

    console.log("minesweeper.js loaded");

    // LEFT CLICK - AJAX reveal
    $(document).on("click", ".cell-button", function (event) {
        event.preventDefault();

        let button = $(this);

        // Added by Angela: flagged cells should not reveal.
        if (button.attr("data-flagged") === "true") {
            console.log("Cell is flagged. Left-click ignored.");
            return;
        }

        let row = button.data("row");
        let col = button.data("col");
        let boardSize = button.data("board-size");
        let difficulty = button.data("difficulty");

        console.log("Left clicked cell:", row, col);

        $.ajax({
            url: "/User/AjaxCellClick",
            type: "POST",
            data: {
                row: row,
                col: col,
                boardSize: boardSize,
                difficulty: difficulty
            },
            success: function (result) {
                if (result.redirectUrl) {
                    window.location.href = result.redirectUrl;
                    return;
                }
                // Updated by Jacob to allow all adjacent cells to be revealed
                $("#game-board").replaceWith(result);
                $("#timestamp").text("Last Updated: " + new Date().toLocaleTimeString());
            },
            error: function (xhr, status, error) {
                console.error("AJAX failed");
                console.error("Status:", status);
                console.error("Error:", error);
                console.error("Response:", xhr.responseText);
            }
        });
    });

    // RIGHT CLICK - AJAX flag
    $(document).on("contextmenu", ".cell-button", function (event) {
        event.preventDefault();

        let button = $(this);

        let row = button.data("row");
        let col = button.data("col");
        let boardSize = button.data("board-size");
        let difficulty = button.data("difficulty");

        $.ajax({
            url: "/User/AjaxToggleFlag",
            type: "POST",
            data: {
                row: row,
                col: col,
                boardSize: boardSize,
                difficulty: difficulty
            },
            success: function (result) {
                if (result.redirectUrl) {
                    window.location.href = result.redirectUrl;
                    return;
                }

                $("#cell-" + row + "-" + col).replaceWith(result);
                $("#timestamp").text("Last Updated: " + new Date().toLocaleTimeString());
            },
            error: function (xhr, status, error) {
                console.error("Flag AJAX failed");
                console.error("Status:", status);
                console.error("Error:", error);
                console.error("Response:", xhr.responseText);
            }
        });
    });

});
