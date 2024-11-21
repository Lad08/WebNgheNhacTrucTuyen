const audioPlayer = document.getElementById('audioPlayer');
const progressBar = document.getElementById('progressBar');
const progressFilled = document.querySelector('.progress-filled');
const currentTimeDisplay = document.getElementById('currentTime');
const durationDisplay = document.getElementById('duration');
const currentSongTitle = document.getElementById('currentSongTitle'); // Thêm dòng này
const searchInput = document.getElementById('searchInput');
const songLists = document.querySelectorAll('.songList'); // Lấy tất cả các danh sách bài hát
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
    audioPlayer.addEventListener('timeupdate', function() {
        if (audioPlayer.duration > 0) {
            const progress = (audioPlayer.currentTime / audioPlayer.duration) * 100;
    progressFilled.style.width = progress + '%'; // Cập nhật chiều rộng của thanh tiến trình
    currentTimeDisplay.textContent = formatTime(audioPlayer.currentTime);
        }
    });

    // Cập nhật thời gian tổng khi bài hát được tải
    audioPlayer.addEventListener('loadedmetadata', function() {
        durationDisplay.textContent = formatTime(audioPlayer.duration);
    });

    // Đặt lại progress bar và thời gian khi bài hát kết thúc
    audioPlayer.addEventListener('ended', function() {
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
    progressBar.addEventListener('click', function(event) {
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

// Search Function
searchInput.addEventListener('input', function () {
    const searchTerm = this.value.toLowerCase(); // Lấy giá trị tìm kiếm và chuyển thành chữ thường

    songLists.forEach(songList => {
        const songs = songList.getElementsByTagName('li'); // Lấy tất cả các bài hát trong danh sách hiện tại

        Array.from(songs).forEach(song => {
            const songTitle = song.querySelector('.music-name').textContent.toLowerCase(); // Lấy tên bài hát
            const artistName = song.querySelector('.artist').textContent.toLowerCase(); // Lấy tên nghệ sĩ

            // Kiểm tra xem tên bài hát hoặc tên nghệ sĩ có chứa từ khóa tìm kiếm không
            if (songTitle.includes(searchTerm) || artistName.includes(searchTerm)) {
                song.style.display = ''; // Hiển thị bài hát nếu tìm thấy
            } else {
                song.style.display = 'none'; // Ẩn bài hát nếu không tìm thấy
            }
        });
    });
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

// Thêm sự kiện cho thanh trượt âm lượng
volumeSlider.addEventListener('input', function () {
    audioPlayer.volume = this.value; // Cập nhật âm lượng của audioPlayer
});