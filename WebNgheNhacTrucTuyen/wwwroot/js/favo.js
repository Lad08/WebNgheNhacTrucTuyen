$(document).on('click', '.toggle-favorite-playlist', function (e) {
    e.preventDefault();
    var button = $(this);
    var playlistId = button.data('id'); // Lấy id của playlist

    $.post('/Playlists/ToggleFavorite', { id: playlistId }, function (response) {
        if (response.success) {
            var icon = button.find('i');
            // Nếu playlist được yêu thích
            if (response.isFavorite) {
                icon.removeClass('fa-regular fa-heart').addClass('fa-solid fa-heart');
                icon.css('color', 'red');
            } else {
                icon.removeClass('fa-solid fa-heart').addClass('fa-regular fa-heart');
                icon.css('color', 'gray');
            }
        } else {
            alert(response.message); // Hiển thị lỗi nếu có
        }
    });
});