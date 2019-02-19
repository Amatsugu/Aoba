var player;
var isPlaying;
var playIcon;
var lastSeek = -1;
var volumeSlider;
var volumeFill;
var volumeValue;
$(document).ready(function(){
	var audio = $("#player");
	playIcon = $("#art #playPause");
	volumeSlider = $("#volume #volumeSlider");
	volumeFill = $("#volume #volumeSlider #volumeFill");
	volumeValue = $("#volume #volumeValue");
	var seekBarFill = $("#seek #progressBar #progressBarFill");
	var seekBar = $("#seek #progressBar");
	var title = $("#info #title");
	var artist = $("#info #artist");
	var album = $("#info #album");
	var time = $("#seek #time");
	var timeLeft = $("#seek #timeRemaining");
	var art = $("#art");
	//playIcon.click(TogglePlayPause);
	art.click(TogglePlayPause);
	playIcon.hide();
	
	var duration = 1;
	player = AV.Player.fromURL(audio.data("uri"));
	player.on('duration', function(d) {
		duration = d;
	});
	//Seek
	seekBar.on('click', function(e){
		e.preventDefault();
		if(!player.buffered)
			return;
		var pos = e.pageX - seekBar.offset().left;
		if(pos == lastSeek)
			return;
		var seekP = pos/seekBar.width();
		if(seekP > 1)
			seekP = 1;
		else if(seekP < 0)
			seekP = 0;
		if(player.buffered > 0)
			player.seek(seekP * duration);
	});
	//Volume Slider
	volumeSlider.on('click', function(e){
		e.preventDefault();
		var pos = e.pageX - seekBar.offset().left;
		var vol = pos/seekBar.width();
		if(vol > 1)
			vol = 1;
		else if(vol < 0)
			vol = 0;
		var volActual = vol * 100;
		setVol(volActual);
	});
	//Meta data loaded
	player.on('metadata', function(m){
		title.text(m.title);
		artist.text(m.artist);
		album.text(m.album);
		if(m.coverArt)
			art.css("background-image", "url("+m.coverArt.toBlobURL()+")");
	});
	//Playback
	player.on('progress', function(p) {
		seekBarFill.css("width", ((p/duration)* 100).toString() + "%");
		time.text(toMinSec(p));
		timeLeft.text(toMinSec(duration-p));
	});
	//Loading Progress
	art.circleProgress({
		value: 0,
		size: 180,
		animation:{ duration: 100},
		fill:{color:"#f1eaf9"}
	});
	var loadProgress = $("#player #loadProgress");
	player.on('buffer', function(p){
		loadProgress.text(Math.round(p) + "%");
		art.circleProgress('value', (p/100));
		if(p >= 100){
			$("#art canvas").fadeOut(400, function(){
				playIcon.fadeIn();
			});
			loadProgress.fadeOut();
		}
	});
	//Load Volume
	if(window.localStorage.getItem("volume") == null)
		setVol(80);
	else
		setVol(Number(window.localStorage.getItem("volume")));
	// TogglePlayPause();
	player.preload();

});

function setVol(vol)
{
	player.volume = vol;
	volumeValue.text(Math.round(vol) + "%");
	volumeFill.css("width", vol + "%");
	window.localStorage.setItem("volume", vol);
}

function toMinSec(t)
{
	var sec = t / 1000;
	var min = Math.floor(sec/60);
	sec = Math.floor(((sec/60) - min)*60);
	if(sec < 10)
		sec = "0" + sec;
	return min + ":" + sec;
}

function TogglePlayPause()
{
	if(!isPlaying)
	{
		playIcon.css("background", "url(/res/img/PauseButton.png)");
		player.play();
	}
	else
	{
		playIcon.css("background", "url(/res/img/PlayButton.png)");
		player.pause();
	}

	isPlaying = !isPlaying;

}
