const audioPlayer = document.getElementById('audioPlayer');
const progressBar = document.getElementById('progressBar');
const progressFilled = document.querySelector('.progress-filled');
const currentTimeDisplay = document.getElementById('currentTime');
const durationDisplay = document.getElementById('duration');
const currentSongTitle = document.getElementById('currentSongTitle'); // Thêm dòng này
const playPauseButton = document.getElementById('playPauseButton');
let isRepeating = false; // Trạng thái lập lại của bài hát
const repeatButton = document.getElementById('repeatButton');
const volumeSlider = document.getElementById('volumeSlider');

// Thêm sự kiện cho các nút phát nhạc
document.querySelectorAll('.play-button').forEach(button => {
    button.addEventListener('click', function () {
        const songTitle = this.getAttribute('data-title');
        audioPlayer.src = this.getAttribute('data-file-path');
        audioPlayer.play();
        currentSongTitle.textContent = "Bài hát hiện tại: " + songTitle;

        // Hiện thanh điều khiển phát nhạc
        document.querySelector('.play-controll').style.display = 'block';

        // Cập nhật trạng thái nút play/pause
        playPauseButton.classList.remove('skipControl_play');
        playPauseButton.classList.add('skipControl_pause');
    });
});

// Cập nhật progress bar và thời gian khi bài hát đang phát
audioPlayer.addEventListener('timeupdate', function () {
    if (audioPlayer.duration > 0) {
        const progress = (audioPlayer.currentTime / audioPlayer.duration) * 100;
        progressFilled.style.width = progress + '%'; // Cập nhật chiều rộng của thanh tiến trình
        currentTimeDisplay.textContent = formatTime(audioPlayer.currentTime);
    }
});

// Cập nhật thời gian tổng khi bài hát được tải
audioPlayer.addEventListener('loadedmetadata', function () {
    durationDisplay.textContent = formatTime(audioPlayer.duration);
});

// Đặt lại progress bar và thời gian khi bài hát kết thúc
audioPlayer.addEventListener('ended', function () {
    progressFilled.style.width = '0%'; // Reset progress bar khi bài hát kết thúc
    currentTimeDisplay.textContent = '00:00'; // Reset thời gian hiện tại
    currentSongTitle.textContent = "Bài hát hiện tại: "; // Reset tên bài hát
});

// Hàm định dạng thời gian
function formatTime(seconds) {
    const minutes = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${minutes}:${secs < 10 ? '0' : ''}${secs}`;
}

// Thêm sự kiện click cho progress bar
progressBar.addEventListener('click', function (event) {
    const rect = progressBar.getBoundingClientRect();
    const offsetX = event.clientX - rect.left;
    const totalWidth = rect.width;
    const percentage = offsetX / totalWidth;
    const newTime = percentage * audioPlayer.duration;
    audioPlayer.currentTime = newTime;

    // Cập nhật progressbar ngay lập tức
    progressFilled.style.width = percentage * 100 + '%'; // Cập nhật giá trị progress bar
    currentTimeDisplay.textContent = formatTime(newTime); // Cập nhật thời gian hiện tại
});

function toggleFavorite(songId) {
    fetch(`/Songs/ToggleFavorite/${songId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => {
            if (response.ok) {
                // Cập nhật giao diện người dùng nếu cần
                location.reload(); // Tải lại trang để cập nhật trạng thái
            }
        })
        .catch(error => console.error('Error:', error));
}

// Sự kiện cho nút play/pause
playPauseButton.addEventListener('click', function () {
    if (audioPlayer.paused) {
        audioPlayer.play();
        playPauseButton.classList.remove('skipControl_play');
        playPauseButton.classList.add('skipControl_pause');
    } else {
        audioPlayer.pause();
        playPauseButton.classList.remove('skipControl_pause');
        playPauseButton.classList.add('skipControl_play');
    }
});


// Thêm sự kiện cho nút Repeat
repeatButton.addEventListener('click', function () {
    isRepeating = !isRepeating; // Đảo ngược trạng thái lặp
    this.classList.toggle('active', isRepeating); // Thay đổi lớp CSS
});

// Cập nhật trạng thái nút Play/Pause khi bài hát kết thúc
audioPlayer.addEventListener('ended', function () {
    if (isRepeating) {
        audioPlayer.currentTime = 0; // Reset về ban đầu
        audioPlayer.play(); // Phát lại bài hát
    }
    playPauseButton.classList.remove('skipControl_pause');
    playPauseButton.classList.add('skipControl_play'); // Đặt lại trạng thái nút
});

// Đặt âm lượng ban đầu
audioPlayer.volume = 1; // Mặc định là âm lượng tối đa

const volumeIcon = document.getElementById('volumeIcon'); // Lấy phần tử icon âm lượng

// Thêm sự kiện cho thanh trượt âm lượng
volumeSlider.addEventListener('input', function () {
    audioPlayer.volume = this.value; // Cập nhật âm lượng của audioPlayer

    // Thay đổi icon dựa vào giá trị âm lượng
    if (audioPlayer.volume === 0) {
        volumeIcon.classList.remove('fa-volume-high');
        volumeIcon.classList.add('fa-volume-xmark'); // Đổi sang icon mute
    } else {
        volumeIcon.classList.remove('fa-volume-xmark');
        volumeIcon.classList.add('fa-volume-high'); // Đổi sang icon âm lượng cao
    }
});

// Bỏ qua 10 giây
document.querySelector('.skipControl_next').addEventListener('click', function () {
    if (audioPlayer.currentTime + 10 <= audioPlayer.duration) {
        audioPlayer.currentTime += 10; // Tiến 10 giây
    } else {
        audioPlayer.currentTime = audioPlayer.duration; // Đến cuối bài hát
    }
});

// Quay lại 10 giây
document.querySelector('.skipControl_previous').addEventListener('click', function () {
    if (audioPlayer.currentTime - 10 >= 0) {
        audioPlayer.currentTime -= 10; // Lùi 10 giây
    } else {
        audioPlayer.currentTime = 0; // Đặt về đầu bài hát
    }
});

document.addEventListener("DOMContentLoaded", function () {
    // Lấy tất cả các hàng trong bảng
    const rows = document.querySelectorAll(".song-row");
    const tableContainer = document.querySelector(".table-container");

    // Thêm sự kiện click vào từng hàng
    rows.forEach(row => {
        row.addEventListener("click", function (event) {
            // Bỏ lớp 'active' trên tất cả các hàng
            rows.forEach(r => r.classList.remove("active"));
            // Thêm lớp 'active' vào hàng được click
            this.classList.add("active");

            // Ngăn không cho sự kiện click lan ra ngoài
            event.stopPropagation();
        });
    });

    // Xóa lớp 'active' khi click ra bên ngoài bảng
    document.addEventListener("click", function () {
        rows.forEach(row => row.classList.remove("active"));
    });

    // Ngăn việc bỏ chọn khi click vào bên trong bảng
    tableContainer.addEventListener("click", function (event) {
        event.stopPropagation();
    });
});

$(document).on('click', '.toggle-favorite', function (e) {
    e.preventDefault();
    var button = $(this);
    var albumId = button.data('id');

    $.post('/Albums/ToggleFavorite', { id: albumId }, function (response) {
        if (response.success) {
            var icon = button.find('i');
            if (response.isFavorite) {
                icon.removeClass('fa-regular fa-heart').addClass('fa-solid fa-heart');
            } else {
                icon.removeClass('fa-solid fa-heart').addClass('fa-regular fa-heart');
            }
        } else {
            alert(response.message);
        }
    });
});


