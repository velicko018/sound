/*
* Fullwidth Audio Player V1.6.2
* Author: Rafael Dery
* Copyright 2011
*
* Only for the sale at the envato marketplaces
*
*/

;(function($) {
	$.fullwidthAudioPlayer = {version: '1.6.1', author: 'Rafael Dery'};

	jQuery.fn.fullwidthAudioPlayer = function(arg) {
		var options = $.extend({},$.fn.fullwidthAudioPlayer.defaults,arg);
		var $elem,
			$wrapper,
			$main,
			$playerWrapper,
			$playerControls,
			$playlistControls,
			$metaWrapper,
			$timeBar,
			$playlistWrapper = null,
		    player,
		    currentTime,
		    totalHeight = 0,
			loadingIndex = -1,
			volumeBarWidth = 50,
			currentIndex = -1,
			currentVolume = 100,
			paused,
			playlistCreated,
			playlistIsOpened = false,
			playAddedTrack = false,
			popupMode = false,
			isPopupWin = false,
			soundcloudKey = 'd2be7a47322c293cdaffc039a26e05d1',
			tracks = [];

		//*********************************************
		//************** LOADING CORE *****************
		//*********************************************

		function _init(elem) {

			if((options.keepClosedOnceClosed || options.storePlaylist) && typeof amplify == 'undefined') {
				alert('Please include the amplify.min.js file in your document!');
				return false;
			}

			//amplify.store('fap-playlist', null);

			$elem = $(elem);
			$elem.hide();

			//check if script is executed in the popup window
			isPopupWin = elem.id == 'fap-popup';

			if(_detectMobileBrowsers()) {
				if(options.hideOnMobile) { return false; }
				//volume and playlist will be also disabled on mobile devices
				options.autoPlay = options.playlist = false;
				options.wrapperPosition = 'top';
			}

			if(typeof amplify != 'undefined' && amplify.store('fap-keep-closed') === 1 && options.keepClosedOnceClosed) {
				options.opened = false;
			}

			//check if a popup window exists
			playlistCreated = Boolean(window.fapPopupWin);
			if(!options.autoPopup) { playlistCreated = true; }
			paused = !options.autoPlay;

			_documentTrackHandler();

			totalHeight = options.playlist ? options.height+options.playlistHeight+options.offset : options.height;

			if(options.wrapperPosition == "popup" && !isPopupWin) {

				options.layout = 'fullwidth';
				popupMode = true;
				if(!options.playlist) { totalHeight = options.height; }
				if(options.autoPopup && !window.fapPopupWin) {
					_addTrackToPopup($elem.html(), options.autoPlay);
				}

				return false;
			}

			//init soundcloud
			if(window.SC) {
				SC.initialize({
					client_id: "d2be7a47322c293cdaffc039a26e05d1"
				});
			}


			var fapDom = '<div id="fap-wrapper"><div id="fap-closer"><i class="fa fa-power-off"></i><i class="fa fa-music"></i></div><div id="fap-main"><div id="fap-wrapper-switcher"><i class="fa fa-list"></i></div><p id="fap-init-text">Creating Playlist...</p></div></div>';

			$('body').append(fapDom);

			$wrapper = $('body').children('#fap-wrapper');
			$main = $wrapper.children('#fap-main');

			if(options.layout == "fullwidth") {
				$wrapper.addClass('fap-fullwidth').css({});
			}
			else if(options.layout == "boxed") {
				$wrapper.addClass('fap-boxed');
				$main.css({});
			}

			if(isPopupWin) {
				$wrapper.addClass('fap-popup-skin');
			}

			//change wrapper css for mobile
			if(_detectMobileBrowsers()) {
				$wrapper.css({})
			}

			//position main wrapper
			if(isPopupWin) {
				$main.css({});
			}
			else if(options.mainPosition == 'center') {
				$main.css({});
			}
			else if(options.mainPosition == 'right') {
				$main.css({});
			}
			else {
				$main.css({});
			}

			//switcher handler
			$("#fap-playlist").hide();

			$('#fap-wrapper-switcher').click(function() {
				$("#fap-playlist").slideToggle(500, 'swing', function(){
					//$('#fap-wrapper-switcher').addClass('fa fa-outdent');
				});
			});
			soundManager.onready(_onSoundManagerReady);
		};

		function _addTrackToPopup(html, playIt, enqueuePageTracks) {

			enqueuePageTracks = typeof enqueuePageTracks !== 'undefined' ? enqueuePageTracks : true;
			selectIndex = typeof selectIndex !== 'undefined' ? selectIndex : 0;

			if( !window.fapPopupWin || window.fapPopupWin.closed ) {

				var windowWidth = 980;
				var centerWidth = (window.screen.width - windowWidth) / 2;
    			var centerHeight = (window.screen.height - totalHeight) / 2;
    			var isChrome = /Chrome/i.test(navigator.userAgent);

				window.fapPopupWin = window.open(options.popupUrl, '', 'menubar=no,toolbar=no,location=yes,status=no,width='+windowWidth+',height='+(isChrome ? totalHeight+30 : totalHeight)+',left='+centerWidth+',top='+centerHeight+'');

				if(window.fapPopupWin == null) {
					alert("Pop-Up Music Player can not be opened. Your browser is blocking Pop-Ups. Please allow Pop-Ups for this site to use the Music Player.");
				}
				$(window.fapPopupWin).load(function() {
					$(window.fapPopupWin).animate({innerHeight: totalHeight}, 500);
					if(enqueuePageTracks) {
						$('.fap-single-track[data-autoenqueue="yes"]').each(function(i, item) {
							var node = $(item);
							html += _createHtmlFromNode(node);
					    });
					}
					options.autoPlay = playIt;
					window.fapPopupWin.initPlayer(options, html);
					playlistCreated = true;
				});

			}
			else {
				var $node = $(html);
				$.fullwidthAudioPlayer.addTrack($node.attr('data-music'), $node.attr('title'), ($node.data('meta') ? $('body').find($node.data('meta')).html() : ''), $node.attr('rel'), $node.attr('target'), playIt);

			}

		}

		function _onSoundManagerReady() {

			if(options.playlist) {
				var playlistDom = '<ul id="fap-playlist"></ul>';
				$playlistWrapper = $(playlistDom);
			}

			if(options.xmlPath) {
				//get playlists from xml file
				$.ajax({ type: "GET", url: options.xmlPath, dataType: "xml", cache: false, success: function(xml) {

					var playlists = $(xml).find('playlists'),
					    playlistId = options.xmlPlaylist ? playlistId = options.xmlPlaylist : playlistId = playlists.children('playlist:first').attr('id');

					_createInitPlaylist(playlists.children('playlist[id="'+playlistId+'"]').children('track'));

					//check if custom xml playlists are set in the HTML document
					$('.fap-xml-playlist').each(function(i, playlist) {
						var $playlist = $(playlist);
						$playlist.append('<h3>'+playlist.title+'</h3><ul class="fap-my-playlist"></ul>');
						//get the start playlist
						playlists.children('playlist[id="'+playlist.id+'"]').children('track').each(function(j, track) {
							var $track = $(track);
							var targetString = $track.attr('target') ? 'target="'+$track.attr('target')+'"' : '';
							var relString = $track.attr('rel') ? 'rel="'+$track.attr('rel')+'"' : '';
							var metaString = $track.find('meta') ? 'data-meta="#'+playlist.id+'-'+j+'"' : '';
							$playlist.children('ul').append('<li><a data-music="'+$track.attr('data-music')+'" title="'+$track.attr('title')+'" '+targetString+' '+relString+' '+metaString+'>'+$track.attr('title')+'</a></li>');
							$playlist.append('<span id="'+playlist.id+'-'+j+'">'+$track.find('meta').text()+'</span>');
						});
					});

				},
				error: function() {
					alert("XML file could not be loaded. Please check the XML path!");
				}
			  });
			}
			else {
				_createInitPlaylist($elem.children('a'));
			}
		};

		function _createInitPlaylist(initTracks) {

			if(options.storePlaylist) {
				var initFromBrowser = Boolean(amplify.store('fap-playlist'));
			}

			initTracks = initFromBrowser ? JSON.parse(amplify.store('fap-playlist')) : initTracks;


			$elem.bind('fap-tracks-stored', function() {

				++loadingIndex;
				if(loadingIndex < initTracks.length) {
					//get stored playlist from browser when available
					if(options.storePlaylist && initFromBrowser) {
						var initTrack = initTracks[loadingIndex];
						$.fullwidthAudioPlayer.addTrack(initTrack.stream_url, initTrack.title, initTrack.meta, initTrack.artwork_url, initTrack.permalink_url, options.autoPlay);
					}
					else {
						var initTrack = initTracks.eq(loadingIndex);

						$.fullwidthAudioPlayer.addTrack(initTrack.attr('data-music'), initTrack.attr('title'), options.xmlPath ? initTrack.children('meta').text() : $elem.find(initTrack.data('meta')).html(), initTrack.attr('rel'), initTrack.attr('target'), options.autoPlay);
					}
				}
				else {
					$elem.unbind('fap-tracks-stored');
					if(options.randomize) { _shufflePlaylist(); }

					_buildPlayer();
				}
			}).trigger('fap-tracks-stored');

		};



		//*********************************************
		//************** DOM INTERFACE ****************
		//*********************************************

		function _buildPlayer() {

			//remove init text
			$main.children('p').remove();

			//create meta wrapper
			$main.append('<div id="fap-meta-wrapper" class="fap-clearfix"><div id="fap-cover-wrapper"><img src="" id="fap-current-cover" /><div id="fap-cover-replacement"></div></div><div id="fap-track-info"><p id="fap-current-title"></p><p id="fap-current-meta"></p></div></div>');

			$metaWrapper = $main.children('#fap-meta-wrapper').css('', options.height-10);

			//add a cover replacement
			_createCoverReplacement(document.getElementById('fap-cover-replacement'), options.coverSize[0], options.coverSize[1]);

			//append social links if requested
			if(options.socials) {
				$metaWrapper.children('#fap-track-info').append('<p id="fap-social-links"><a data-music="" target="_blank">'+options.facebookText+'</a><a data-music="" target="_blank">'+options.twitterText+'</a><a data-music="" target="_blank">'+options.downloadText+'</a><a data-music="" target="_blank" class="fap-soundcloud-link"></a></p>');
			}

			//create ui wrapper
			$playerWrapper = $main.append('<div id="fap-player-wrapper" class="fap-clearfix"><div id="fap-player-controls"></div><div id="fap-playlist-controls"></div></div>').children('#fap-player-wrapper');
			$playerControls = $playerWrapper.children('#fap-player-controls');
			$playlistControls = $playerWrapper.children('#fap-playlist-controls');

			//append UI Wrapper
			var $mainControls = $playerControls.append('<div id="fap-main-controls"></div>').children('#fap-main-controls');

			//append previous button
			$mainControls.append('<a data-music="#" id="fap-previous" class="fa fa-caret-left"></a>').children('#fap-previous').click(function() {
				$.fullwidthAudioPlayer.previous();
				return false;
			});

			//append play/pause button
			$mainControls.append('<a data-music="#" id="fap-play-pause" class="fa fa-play"></a>').children('#fap-play-pause').click(function() {
				$.fullwidthAudioPlayer.toggle();
				return false;
			});

			//append next button
			$mainControls.append('<a data-music="#" id="fap-next" class="fa fa-caret-right"></a>').children('#fap-next').click(function() {
				$.fullwidthAudioPlayer.next();
				return false;
			});

			//append volume bar if requested - hidden for mobile browsers
			if(options.volume) {
				$playerControls.append('<div id="fap-volume-bar" class="fap-clearfix"><div id="fap-volume-scrubber"><div id="fap-volume-indicator"></div></div><div id="fap-volume-sign" class="fa fa-volume-up"></div></div>');

				$playerControls.find('#fap-volume-scrubber').click(function(evt) {
					var value = (evt.pageX - $(this).offset().left) / volumeBarWidth;
					$.fullwidthAudioPlayer.volume(value);
				});

				$playerControls.find('#fap-volume-sign').dblclick(function() {
					if($(this).hasClass('fa-volume-up')) {
						$.fullwidthAudioPlayer.volume(0);
					}
					else {
						$.fullwidthAudioPlayer.volume(100);
					}
				});

			}

			//append time bar
			$timeBar = $playerControls.append('<div id="fap-time-bar" class="fap-clearfix"><div id="fap-loading-bar"></div><div id="fap-progress-bar"></div><span id="fap-current-time">00:00:00</span><span id="fap-total-time">00:00:00</span></div>').children('#fap-time-bar');

			$playerControls.find('#fap-loading-bar, #fap-progress-bar').click(function(evt) {
				var progress = (evt.pageX - $(this).parent().offset().left) / $timeBar.width();
				player.setPosition(progress * player.duration);
				_setSliderPosition(progress);
			});

			//append popup button if requested
			if(options.popup && !isPopupWin) {

				$playlistControls.append('<a data-music="#" id="fap-player-popup" class="fa fa-caret-square-o-up" title="Pop-up Player"></a>')
				.children('#fap-player-popup').click(function(evt) {

					popupMode = true;
					options.selectedIndex = currentIndex;

					var html = '';
					for(var i=0; i < tracks.length; ++i) {
						var track = tracks[i];
						html += '<a data-music="'+(track.permalink ? track.permalink_url : track.stream_url)+'" title="'+(track.title)+'" target="'+(track.permalink_url)+'" rel="'+(track.artwork_url)+'"></a>';

						if(track.meta && track.meta.length) {
							html += '<span>'+(track.meta)+'</span>';
						}
					}

					_addTrackToPopup(html, !paused, false);

					$.fullwidthAudioPlayer.clear();
					$wrapper.remove();

					evt.preventDefault();

				});

			}

			//append shuffle button if requested
			if(options.shuffle) {

				$playlistControls.append('<a data-music="#" id="fap-playlist-shuffle" class="fa fa-random"></a>')
				.children('#fap-playlist-shuffle').click(function(evt) {
					_shufflePlaylist();

					evt.preventDefault();
				});

			}

			//create visual playlist if requested - hidden for mobile browsers
			if(options.playlist) {

				if(options.wrapperPosition == 'bottom') {
					$main.append('<div class="clear"></div>').append($playlistWrapper);
					$playlistWrapper.css({});
				}
				else {
					$main.prepend('<div class="clear"></div>').prepend($playlistWrapper);
					$playlistWrapper.css({})
				}

				//make playlist sortable
				if(options.sortable) {
					var oldIndex;
					$playlistWrapper.sortable().bind('sortstart', function(evt, ui) {
						ui.item.addClass('fap-prevent-click');
						oldIndex = $playlistWrapper.children('li').index(ui.item);
					});

					$playlistWrapper.sortable().bind('sortupdate', function(evt, ui) {
						var targetIndex = $playlistWrapper.children('li').index(ui.item);
						var item = tracks[oldIndex];
						var currentTitle = tracks[currentIndex].title;
						tracks.splice(oldIndex, 1);
						tracks.splice(targetIndex, 0, item);
						_updateTrackIndex(currentTitle);
						if(options.storePlaylist) { amplify.store('fap-playlist', JSON.stringify(tracks)); }
					});
				}

				if(!isPopupWin) {
					//playlist switcher
					$playlistControls.append('<a data-music="#" id="fap-playlist-toggle" class="fa fa-list" title="Playlist"></a>')
					.children('#fap-playlist-toggle').click(function(evt) {
						playlistIsOpened ? $.fullwidthAudioPlayer.setPlayerPosition('closePlaylist', true) : $.fullwidthAudioPlayer.setPlayerPosition('openPlaylist', true);

						evt.preventDefault();
					});

				}
				else {
					//open playlist when player is in the pop-up window
					$.fullwidthAudioPlayer.setPlayerPosition('openPlaylist', false);
				}

			}

			//register keyboard events
			if(options.keyboard) {
				$(document).keyup(function(evt) {
					switch (evt.which) {
						case 32:
						$.fullwidthAudioPlayer.toggle();
						break;
						case 39:
						$.fullwidthAudioPlayer.next();
						break;
						case 37:
						$.fullwidthAudioPlayer.previous();
						break;
						case 38:
						$.fullwidthAudioPlayer.volume((currentVolume / 100)+.05);
						break;
						case 40:
						$.fullwidthAudioPlayer.volume((currentVolume / 100)-.05);
						break;
					}
				});
			}

			//add margin for p elements in meta wrapper
			$metaWrapper.children('p').css('', options.coverSize[0] + 10);

			//fire on ready handler
			$elem.trigger('onFapReady');
			playlistCreated = true;

			var autoEnqueuedTracks = $('.fap-single-track[data-autoenqueue="yes"]');
			loadingIndex = -1;
			function _addAutoEnqueuedTrack() {
				++loadingIndex;
				if(loadingIndex < autoEnqueuedTracks.size()) {
					var $track = autoEnqueuedTracks.eq(loadingIndex);
					$.fullwidthAudioPlayer.addTrack($track.attr('data-music'), $track.attr('title'), $('body').find($track.data('meta')).html(), $track.attr('rel'), $track.attr('target'), false);
				}
				else {
					$elem.unbind('fap-tracks-stored', _addAutoEnqueuedTrack);
				}
			};

			$elem.bind('fap-tracks-stored', _addAutoEnqueuedTrack);
			_addAutoEnqueuedTrack();

			//start playing track when addTrack method is called
			$elem.bind('fap-tracks-stored', function(evt, trackIndex) {
				if(playAddedTrack) { $.fullwidthAudioPlayer.selectTrack(trackIndex, playAddedTrack); }
			});

			//select first track when playlist has tracks
		    $.fullwidthAudioPlayer.selectTrack(options.selectedIndex, /Android|webOS|iPhone|iPod|iPad|BlackBerry/i.test(navigator.userAgent) ? false : options.autoPlay);
			options.autoPlay ? $elem.trigger('onFapPlay') : $elem.trigger('onFapPause');

		};

		function _documentTrackHandler() {

			if($elem.jquery >= "1.7"){
				$('body').on('click', '.fap-my-playlist li a, .fap-single-track', _addTrackFromDocument);
				$('body').on('click', '.fap-add-playlist', _addTrackFromDocument);
			}
			else {
				$('body').delegate('.fap-my-playlist li a, .fap-single-track', 'click', _addTrackFromDocument);
				$('body').delegate('.fap-add-playlist', 'click', _addTrackFromDocument);
			}

			function _addTrackFromDocument() {
				if(!playlistCreated) { return false; }
				var node = $(this),
					playIt = true;

				if(node.data('enqueue')) {
					playIt = node.data('enqueue') == 'yes' ? false : true;
				}

				if(popupMode) {
					//adding whole plalist to the player
					if(node.hasClass('fap-add-playlist')) {
						var playlistId = node.data('playlist'),
							tracks = jQuery('[data-playlist="'+playlistId+'"]').find('.fap-single-track'),
							html = _createHtmlFromNode($(tracks.get(0)));

						if(tracks.size() == 0) { return false; }

						//add first track to pop-up to open it
						_addTrackToPopup(html, playIt);
						tracks.splice(0, 1);

						window.fapReady = window.fapPopupWin.addTrack != undefined;
						//start interval for adding the playlist into the pop-up player
						var interval = setInterval(function() {
							if(window.fapReady) {
								clearInterval(interval);
								tracks.each(function(i, item) {
									_addTrackToPopup(item, playIt);
							    });
							}
						}, 50);
					}
					//adding a single track to the player
					else {
						var html = _createHtmlFromNode(node);
						_addTrackToPopup(html, playIt);
					}

				}
				else {
					//adding whole plalist to the player
					if(node.hasClass('fap-add-playlist')) {
						var playlistId = node.data('playlist'),
							tracks = jQuery('[data-playlist="'+playlistId+'"]').find('.fap-single-track');

						if(tracks.size() == 0) { return false; }

						loadingIndex = -1;
						function _addTrackFromPlaylist() {
							++loadingIndex;
							if(loadingIndex < tracks.size()) {
								var $track = tracks.eq(loadingIndex);
								$.fullwidthAudioPlayer.addTrack($track.attr('data-music'), $track.attr('title'), $('body').find($track.data('meta')).html(), $track.attr('rel'), $track.attr('target'), (loadingIndex == 0 && playIt));
							}
							else {
								$elem.unbind('fap-tracks-stored', _addTrackFromPlaylist);
							}
						};

						$elem.bind('fap-tracks-stored', _addTrackFromPlaylist);
						_addTrackFromPlaylist();

					}
					//adding a single track to the player
					else {
						$.fullwidthAudioPlayer.addTrack(node.attr('data-music'), node.attr('title'), $('body').find(node.data('meta')).html(), node.attr('rel'), node.attr('target'), playIt);
					}

				}

				return false;
			};

		};


		//*********************************************
		//************** API METHODS ******************
		//*********************************************

		//global method for playing the current track
		$.fullwidthAudioPlayer.play = function() {
			if(currentIndex == -1) {
				$.fullwidthAudioPlayer.next();
			}
			if(tracks.length > 0) {
				if(player.playState) {
					player.resume();
				}
				else {
				    player.play();
				}

				$playerControls.find('#fap-play-pause').removeClass('fa-play').addClass('fa-pause');
				paused = false;
				$elem.trigger('onFapPlay');
			}
		};

		//global method for pausing the current track
		$.fullwidthAudioPlayer.pause = function() {
			if(tracks.length > 0) {
				player.pause();
				$playerControls.find('#fap-play-pause').removeClass('fa-pause').addClass('fa-play');
				paused = true;
				$elem.trigger('onFapPause');
			}
		};

		//global method for pausing/playing the current track
		$.fullwidthAudioPlayer.toggle = function() {
			if(paused) {
				$.fullwidthAudioPlayer.play();
			}
			else {
				$.fullwidthAudioPlayer.pause();
			}
		};

		//global method for playing the previous track
		$.fullwidthAudioPlayer.previous = function() {
			if(tracks.length > 0) {
				$.fullwidthAudioPlayer.selectTrack(currentIndex-1, true);
			}
		};

		//global method for playing the next track
		$.fullwidthAudioPlayer.next = function() {
			if(tracks.length > 0) {
				$.fullwidthAudioPlayer.selectTrack(currentIndex+1, true);
			}
		};

		$.fullwidthAudioPlayer.volume = function(value) {
			if(tracks.length > 0) {
				if(value < 0 ) value = 0;
				if(value > 1 ) value = 1;
				currentVolume = value * 100;
				if(player) { player.setVolume(currentVolume); }
				$playerControls.find('#fap-volume-indicator').width(value * volumeBarWidth);
				if(value == 0) {
					$playerControls.find('#fap-volume-sign').removeClass('fa-volume-up').addClass('fa-volume-off');
				}
				else {
					$playerControls.find('#fap-volume-sign').removeClass('fa-volume-off').addClass('fa-volume-up');
				}
			}
		};

		//global method for adding a track to the playlist
		$.fullwidthAudioPlayer.addTrack = function(trackUrl, title, meta, cover, linkUrl, playIt) {
			if(trackUrl == null || trackUrl == '') {
				alert('The track with the title "'+title+'" does not contain a track resource!');
				return false;
			}

			if ( title === undefined ) {
			   title = '';
			}
			if ( meta === undefined ) {
			   meta = '';
			}
			if ( cover === undefined ) {
			   cover = '';
			}
			if ( linkUrl === undefined ) {
			   linkUrl = '';
			}
			if ( playIt === undefined ) {
			   playIt = false;
			}

			if(popupMode && window.fapPopupWin && !window.fapPopupWin.closed) {
				window.fapPopupWin.addTrack(trackUrl,title,meta,cover,linkUrl, playIt);
				window.fapPopupWin.focus();
				return false;
			}

			var base64Matcher = new RegExp("^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=|[A-Za-z0-9+/]{4})$");
			if(base64Matcher.test(trackUrl)) {
				trackUrl = Base64.decode(trackUrl);
			}

			playAddedTrack = playIt;
			if(RegExp('http(s?)://soundcloud').test(trackUrl) || RegExp('http(s?)://official.fm').test(trackUrl)) {
				_getTracksFromExternalSource(trackUrl);
			}
			else {
				var li = _storeTrackDatas({stream_url: trackUrl, title: title, meta: meta, artwork_url: cover, permalink_url:linkUrl});
				$elem.trigger('onFapTracksAdded', [tracks]);
				$elem.trigger('fap-tracks-stored', [li]);
			}

		};

		//select a track by index
		$.fullwidthAudioPlayer.selectTrack = function(index, playIt) {

			if(tracks.length <= 0) {
				$.fullwidthAudioPlayer.clear();
				return false;
			}

			if(index == currentIndex) {
				$.fullwidthAudioPlayer.toggle();
				return false;
			}
			else if(index < 0) { currentIndex = tracks.length - 1; }
			else if(index >= tracks.length) {
				currentIndex = 0;
				playIt = options.loopPlaylist;
			}
			else { currentIndex = index; }

			paused = !playIt;

			var isSoundcloud = RegExp('http(s?)://soundcloud').test(tracks[currentIndex].permalink_url);

			//reset
			$playerControls.find('#fap-progress-bar').width(0);
			$playerControls.find('#fap-total-time, #fap-current-time').text('00:00:00');

			$metaWrapper.find('#fap-current-cover').attr('src', tracks[currentIndex].artwork_url);
			$metaWrapper.find('#fap-current-title').html(tracks[currentIndex].title);
			$metaWrapper.find('#fap-current-meta').html(isSoundcloud ? tracks[currentIndex].genre : tracks[currentIndex].meta);

			if(!tracks[currentIndex].artwork_url) {
				$metaWrapper.find('#fap-current-cover').hide();
				$metaWrapper.find('#fap-cover-replacement').show();
			}
			else {
				$metaWrapper.find('#fap-current-cover').show();
				$metaWrapper.find('#fap-cover-replacement').hide();
			}

			if(tracks[currentIndex].permalink_url) {
				$metaWrapper.find('#fap-social-links').children('a').show();
				var facebookLink = 'http://www.facebook.com/sharer.php?u='+encodeURIComponent(tracks[currentIndex].permalink_url);
				var twitterLink = 'http://twitter.com/share?url='+encodeURIComponent(tracks[currentIndex].permalink_url)+'&text='+encodeURIComponent(tracks[currentIndex].title)+'';

				$metaWrapper.find('#fap-social-links a:eq(0)').attr('href', facebookLink);
				$metaWrapper.find('#fap-social-links a:eq(1)').attr('href', twitterLink);
				$metaWrapper.find('#fap-social-links a:eq(2)').attr('href', tracks[currentIndex].permalink_url+'/download');
				$metaWrapper.find('#fap-social-links a:eq(3)').attr('href', tracks[currentIndex].permalink_url);
			}
			else {
				$metaWrapper.find('#fap-social-links').children('a').hide();
			}

			if($playlistWrapper) {
				$playlistWrapper.children('li').removeClass('active');
				$playlistWrapper.children('li').eq(currentIndex).addClass('active');
				

				
			}

			if(playIt) {
				$playerControls.find('#fap-play-pause').removeClass('fa-play').addClass('fa-pause');
			}
			else {
				$playerControls.find('#fap-play-pause').removeClass('fa-pause').addClass('fa-play');
			}

			//destroy sound
			if(player) {
				player.destruct();
			}

			//options for soundmanager
			var soundManagerOptions = {
				id: 'fap_sound',
				autoPlay: playIt,
				autoLoad: options.autoLoad,
				volume: currentVolume,
				whileloading: _onLoading,
				whileplaying: _onPlaying,
				onfinish: _onFinish,
				onload: _onLoad
			};

			if(isSoundcloud) {
				$metaWrapper.find('#fap-social-links').children('a:eq(3)').show();
				if(tracks[currentIndex].downloadable) { $metaWrapper.find('#fap-social-links').children('a:eq(2)').show(); }
				else { $metaWrapper.find('#fap-social-links').children('a:eq(2)').hide(); }

				$.extend(soundManagerOptions, {id: "fap_sound", url: tracks[currentIndex].stream_url+'?client_id='+SC.options.client_id});
				/*SC.stream(tracks[currentIndex].stream_url,
					soundManagerOptions,
					function(sound){
						if(sound) {
							player = sound;
						}
						else {
							alert("Streaming could not be started. Please try again!");
						}
				  	}
				);*/
			}
			else {
				$metaWrapper.find('#fap-social-links').children('a:eq(2), a:eq(3)').hide();
				$.extend(soundManagerOptions, {id: "fap_sound", url: tracks[currentIndex].stream_url});
			}

			player = soundManager.createSound(soundManagerOptions);

			if(!options.opened && (playIt && options.openPlayerOnTrackPlay)  && !isPopupWin ) {
				$.fullwidthAudioPlayer.setPlayerPosition('open', true);
			}

			$elem.trigger('onFapTrackSelect', [ tracks[currentIndex], playIt ]);

		};


		//removes all tracks from the playlist and stops playing - states: open, close, openPlaylist, closePlaylist
		$.fullwidthAudioPlayer.setPlayerPosition = function(state, animated) {

			if($wrapper.is(':animated')) { return false; }

			if(state == "open") {
				$main.children('#fap-wrapper-switcher').html(options.closeLabel);
				if(options.wrapperPosition == 'top') {
					if(options.animatePageOnPlayerTop) {
						$('body').animate({}, animated ? 300 : 0);
					}
					$wrapper.animate({'top': -(totalHeight-options.height)}, animated ? 300 : 0);
				}
				else {
					$wrapper.animate({'bottom': -(totalHeight-options.height)}, animated ? 300 : 0);
				}
				options.opened = true;
			}
			else if(state == "close") {
				$main.children('#fap-wrapper-switcher').html(options.openLabel);
				if(options.wrapperPosition == 'top') {
					if(options.animatePageOnPlayerTop) {
						$('body').animate({}, animated ? 300 : 0);
					}
					$wrapper.animate({'top': -totalHeight-1}, animated ? 300 : 0);
				}
				else {
					$wrapper.animate({'bottom': -totalHeight-1}, animated ? 300 : 0);
				}
				options.opened = playlistIsOpened = false;
			}
			


		};

		//removes all tracks from the playlist and stops playing
		$.fullwidthAudioPlayer.clear = function() {

			//reset everything
			$metaWrapper.find('#fap-current-cover').hide();
			$metaWrapper.find('#fap-cover-replacement').show();
			$metaWrapper.find('#fap-current-title, #fap-current-meta').html('');
			$metaWrapper.find('#fap-social-links').children('a').attr('data-music', '').hide();
			$playerControls.find('#fap-progress-bar, #fap-loading-bar').width(0);
			$playerControls.find('#fap-current-time, #fap-total-time').text('00:00:00');
			$playerControls.find('#fap-play-pause').removeClass('fa-pause').addClass('fa-play');

			paused = true;
			currentIndex = -1;

			if($playlistWrapper) {
			    $playlistWrapper.empty();
			}
			tracks = [];
			if(player) { player.destruct(); }

	

			$elem.trigger('onFapClear');

		};

		//pop up player
		$.fullwidthAudioPlayer.popUp = function(enqueuePageTracks) {

			enqueuePageTracks = typeof enqueuePageTracks !== 'undefined' ? enqueuePageTracks : true;

			if(popupMode) {
				if(!window.fapPopupWin || window.fapPopupWin.closed) {
					_addTrackToPopup('', false, enqueuePageTracks);
				}
				else {
					window.fapPopupWin.focus();
				}
			}

		};


		//*********************************************
		//************** PRIVATE METHODS ******************
		//*********************************************

		function _createHtmlFromNode(node) {
			var html = '<a data-music="'+node.attr('data-music')+'" title="'+(node.attr('title') ? node.attr('title') : '')+'" target="'+(node.attr('target') ? node.attr('target') : '')+'" rel="'+(node.attr('rel') ? node.attr('rel') : '')+'" data-meta="'+(node.data('meta') ? node.data('meta') : '')+'"></a>';
			if(node.data('meta')) {
				var metaText = $('body').find(node.data('meta')).html() ? $('body').find(node.data('meta')).html() : '';
				html += '<span id="'+node.data('meta').substring(1)+'">'+metaText+'</span>';
			}
			return html;
		};

		//get track(s) from soundcloud link
		function _getTracksFromExternalSource(linkUrl) {

			if(RegExp('http(s?)://soundcloud').test(linkUrl)) {
				//replace likes with favorites
				//linkUrl = linkUrl.replace("/likes", "/favorites");

				//load soundcloud data from tracks
	            SC.get('/resolve', {url: linkUrl}, function(data, error){

					var loadIndex = -1, temp = -1;
	            	if(error == null) {

		            	//favorites(likes)
		            	if($.isArray(data)) {
			            	for(var i=0; i < data.length; ++i) {
								temp = _storeTrackDatas(data[i]);
								loadIndex = temp < loadIndex ? temp : loadIndex;
								if(i == 0) { loadIndex = temp; }
							}
		            	}
		            	//sets
		            	else if(data.kind == "playlist") {
			            	for(var i=0; i < data.tracks.length; ++i) {
								temp = _storeTrackDatas(data.tracks[i]);
								loadIndex = temp < loadIndex ? temp : loadIndex;
								if(i == 0) { loadIndex = temp; }
							}
		            	}
		            	//user tracks
		            	else if(data.kind == "user") {
			            	SC.get("/users/"+data.id+"/tracks", function(data, error){

			            		for(var i=0; i < data.length; ++i) {
									temp = _storeTrackDatas(data[i]);
									loadIndex = temp < loadIndex ? temp : loadIndex;
									if(i == 0) { loadIndex = temp; }
								}
								$elem.trigger('onFapTracksAdded', [tracks]);
								$elem.trigger('fap-tracks-stored', [loadIndex]);
			            	});
		            	}
		            	//group tracks
		            	else if(data.kind == "group") {
			            	SC.get("/groups/"+data.id+"/tracks", function(data, error){
			            		for(var i=0; i < data.length; ++i) {
									temp = _storeTrackDatas(data[i]);
									loadIndex = temp < loadIndex ? temp : loadIndex;
									if(i == 0) { loadIndex = temp; }
								}
								$elem.trigger('onFapTracksAdded', [tracks]);
								$elem.trigger('fap-tracks-stored', [loadIndex]);
			            	});
		            	}
		            	//single track
		            	else {
			            	if(data.kind == "track") {
				            	loadIndex = _storeTrackDatas(data);
			            	}
		            	}
	            	}
	            	else {
		            	loadIndex++;
	            	}

	            	if(loadIndex >= 0) {
		            	$elem.trigger('onFapTracksAdded', [tracks]);
						$elem.trigger('fap-tracks-stored', [loadIndex]);
	            	}

	            });
			}
			else if(RegExp('http(s?)://official.fm').test(linkUrl)) {
				var trackId = linkUrl.substr(linkUrl.lastIndexOf('/tracks')+8);
				$.getJSON('http://api.official.fm/tracks/'+trackId+'?fields=streaming,cover&api_version=2', function(data) {
					var track = data.track;
					var li = _storeTrackDatas({stream_url: track.streaming.http, title: track.artist + ' - ' + track.title, meta: track.project.name, artwork_url: track.cover.urls.small, permalink_url:track.page});
					$elem.trigger('onFapTracksAdded', [tracks]);
					$elem.trigger('fap-tracks-stored', [li]);
				});

			}

		};

		//store track datas from soundcloud
		function _storeTrackDatas(data) {
			//search if a track with a same title already exists
			var trackIndex = tracks.length;
			for(var i= 0; i < tracks.length; ++i) {
				if(data.stream_url == tracks[i].stream_url) {
					trackIndex = i;
					return trackIndex;
					break;

				}
			}

			tracks.push(data);
			_createPlaylistTrack(data.artwork_url, data.title);

			if(options.storePlaylist) { amplify.store('fap-playlist', JSON.stringify(tracks)); }

			return trackIndex;
		};


		//soundmanager loading
		function _onLoading() {
			$playerControls.find('#fap-loading-bar').width(Math.round( this.bytesLoaded / this.bytesTotal) * 100+'%');
		};

		//soundmaanger playing
		function _onPlaying() {
			_setTimes(this.position, this.duration);
		};

		//soundmanager finish
		function _onFinish() {
			if(options.playNextWhenFinished) {
				$.fullwidthAudioPlayer.next();
			}
			else {
				$.fullwidthAudioPlayer.pause();
				player.setPosition(0);
				_setSliderPosition(0);
			}
		};

		//soundmanager file load
		function _onLoad(state) {
			if(!state) {
				if(window.console && window.console.log) {
					console.log("Track could not be loaded! Please check the URL: "+this.url);
				}
			}
		};

		//create a new playlist item in the playlist
		function _createPlaylistTrack(cover, title) {

			if(!options.playlist) { return false; }
            var coverDom = cover ? '<img src="'+cover+'" />' : '<div class="fap-cover-replace-small"></div>';

			$playlistWrapper.append('<li class="fap-clearfix">'+coverDom+'<span>'+title+'</span><div class="fap-remove-track">&times;</div></li>');
			var listItem = $playlistWrapper.children('li').last().css({});

			if(navigator.appVersion.indexOf("MSIE 7.")==-1) {
				if(!cover) { _createCoverReplacement(listItem.children('.fap-cover-replace-small').get(0), 20, 20); }
			}

			//Playlist Item Event Handlers
			if($elem.jquery >= "1.7"){
				listItem.on('click', 'span', _selectTrackFromPlaylist);
				listItem.on('click', '.fap-remove-track', _removeTrackFromPlaylist);
			}
			else {
				listItem.delegate('span', 'click', _selectTrackFromPlaylist);
				listItem.delegate('.fap-remove-track', 'click', _removeTrackFromPlaylist);
			}

			function _selectTrackFromPlaylist() {
				var $listItem = $(this).parent();
				if($listItem.hasClass('fap-prevent-click')) {
					$listItem.removeClass('fap-prevent-click');
				}
				else {
					var index = $playlistWrapper.children('li').index($listItem);
					$.fullwidthAudioPlayer.selectTrack(index, true);
				}
			};

			function _removeTrackFromPlaylist() {
				var $this = $(this),
					index = $this.parent().parent().children('li').index($this.parent());

				tracks.splice(index, 1);
				$this.parent().remove();

				if(index == currentIndex) {
					currentIndex--;
					index = index == tracks.length ? 0 : index;
				    $.fullwidthAudioPlayer.selectTrack(index, paused ? false : true);
				}
				else if(index < currentIndex) {
					currentIndex--;
				}
				
			};
		};

		//creates a cover replacement when track has no artwork image
		function _createCoverReplacement(container, width, height) {
			$(container).append('<span>&hellip;</span>');
		};

		//set the time slider position
		function _setSliderPosition(playProgress) {
		    $playerControls.find('#fap-progress-bar').width(playProgress * $timeBar.width());
		};

		//update the current and total time
		function _setTimes(position, duration) {
			var time = _convertTime(position/1000);
			if(currentTime != time) {
				$playerControls.find('#fap-current-time').text(time);
				$playerControls.find('#fap-total-time').text(_convertTime(duration / 1000));
				_setSliderPosition(position / duration);
			}
			currentTime = time;
		};

		//converts seconds into a well formatted time
		function _convertTime(second) {
			second = Math.abs(second);
			var val = new Array();
			val[0] = Math.floor(second/3600%24);//hours
			val[1] = Math.floor(second/60%60);//mins
			val[2] = Math.floor(second%60);//secs
			var stopage = true;
			var cutIndex  = -1;
			for(var i = 0; i < val.length; i++) {
				if(val[i] < 10) val[i] = "0" + val[i];
				if( val[i] == "00" && i < (val.length - 2) && !stopage) cutIndex = i;
				else stopage = true;
			}
			val.splice(0, cutIndex + 1);
			return val.join(':');
		};

		function _shufflePlaylist() {
			if($playlistWrapper) {
				$playlistWrapper.empty();
			}
			//action for the shuffle button
			if(currentIndex != -1) {
				var tempTitle = tracks[currentIndex].title;
				tracks.shuffle();
				_updateTrackIndex(tempTitle);
				for(var i=0; i < tracks.length; ++i) {
					_createPlaylistTrack(tracks[i].artwork_url, tracks[i].title);
				}
				$main.find('#fap-playlist').children('li').eq(currentIndex).css();
				$main.find('#fap-playlist').scrollTop(0);
			}
			//action for randomize option
			else {
				tracks.shuffle();
				for(var i=0; i < tracks.length; ++i) {
					_createPlaylistTrack(tracks[i].artwork_url, tracks[i].title);
				}

			}
			if(options.storePlaylist) { amplify.store('fap-playlist', JSON.stringify(tracks)); }
		};

		//array shuffle
		function _arrayShuffle(){
		  var tmp, rand;
		  for(var i =0; i < this.length; i++){
			rand = Math.floor(Math.random() * this.length);
			tmp = this[i];
			this[i] = this[rand];
			this[rand] = tmp;
		  }
		};
		Array.prototype.shuffle = _arrayShuffle;

		function _updateTrackIndex(title) {
			for(var i=0; i < tracks.length; ++i) {
				if(tracks[i].title == title) { currentIndex = i; }
			}
		};

		function _utf8_decode(utftext) {
		    var string = "";
		    var i = 0;
		    var c = c1 = c2 = 0;

		    while ( i < utftext.length ) {

		        c = utftext.charCodeAt(i);

		        if (c < 128) {
		            string += String.fromCharCode(c);
		            i++;
		        }
		        else if((c > 191) && (c < 224)) {
		            c2 = utftext.charCodeAt(i+1);
		            string += String.fromCharCode(((c & 31) << 6) | (c2 & 63));
		            i += 2;
		        }
		        else {
		            c2 = utftext.charCodeAt(i+1);
		            c3 = utftext.charCodeAt(i+2);
		            string += String.fromCharCode(((c & 15) << 12) | ((c2 & 63) << 6) | (c3 & 63));
		            i += 3;
		        }

		    }

		    return string;
		};

		function _detectMobileBrowsers() {
			
		};

		return this.each(function() {_init(this)});
	};


	//OPTIONS
	$.fn.fullwidthAudioPlayer.defaults = {
		wrapperPosition: 'bottom', //top, bottom or popup
		mainPosition: 'center', //left, center or right
		wrapperColor: '#f0f0f0', //background color of the wrapper
		mainColor: '#3c3c3c',
		fillColor: '#e3e3e3',
		metaColor: '#666666',
		strokeColor: '#e0e0e0',
		activeTrackColor: '#E8E8E8',
		twitterText: 'Share on Twitter',
		facebookText: 'Share on Facebook',
		downloadText: 'Download',
		layout: 'fullwidth', //V1.5 - fullwidth or boxed
		popupUrl: 'popup.html', //- since V1.3
		height: 80, // the height of the wrapper
		playlistHeight: 210, //set the playlist height for the scrolling
		coverSize: [65, 65], //size (width,height) of the cover
		offset: 20, //offset between playlist and upper content
		opened: true, //default state - opened or closed
		volume: true, // show/hide volume control
		playlist: true, //show/hide playlist
		autoLoad: true, //loads the music file when soundmanager is ready
		autoPlay: false, //enable/disbale autoplay
		playNextWhenFinished: true, //plays the next track when current one has finished
		keyboard: true, //enable/disable the keyboard shortcuts
		socials: true, //hide/show social links
		autoPopup: false, //pop out player in a new window automatically - since V1.3
		randomize: false, //randomize default playlist - since V1.3
		shuffle: true, //show/hide shuffle button - since V1.3
		sortable: false, //sortable playlist
		base64: true, //set to true when you encode your mp3 urls with base64
		xmlPath: '', //the xml path
		xmlPlaylist: '', //the ID of the playlist which should be loaded into player from the XML file
		hideOnMobile: false, //1.4.1 - Hide the player on mobile devices
		loopPlaylist: true, //1.5 - When end of playlist has been reached, start from beginning
		storePlaylist: false, //1.5 - Stores the playlist in the browser
		keepClosedOnceClosed: false, //1.6 - Keeps the player closed, once the user closed it
		animatePageOnPlayerTop: false, //1.6.1 - moves the whole page when the player is at the top, so the player does not overlap anything from the page
		openLabel: '+', //1.6.1 - the label for the close button
		closeLabel: '&times;', //1.6.1 - the label for the open button
		openPlayerOnTrackPlay: false, //1.6.1 - opens the player when a track starts playing
		popup: true, //1.6.1 - enable popup button in the player
		selectedIndex: 0 // 1.6.1 - set start track by index when the player is created
	};

})(jQuery);


var Base64={_keyStr:"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=",encode:function(e){var t="";var n,r,i,s,o,u,a;var f=0;e=Base64._utf8_encode(e);while(f<e.length){n=e.charCodeAt(f++);r=e.charCodeAt(f++);i=e.charCodeAt(f++);s=n>>2;o=(n&3)<<4|r>>4;u=(r&15)<<2|i>>6;a=i&63;if(isNaN(r)){u=a=64}else if(isNaN(i)){a=64}t=t+this._keyStr.charAt(s)+this._keyStr.charAt(o)+this._keyStr.charAt(u)+this._keyStr.charAt(a)}return t},decode:function(e){var t="";var n,r,i;var s,o,u,a;var f=0;e=e.replace(/[^A-Za-z0-9\+\/\=]/g,"");while(f<e.length){s=this._keyStr.indexOf(e.charAt(f++));o=this._keyStr.indexOf(e.charAt(f++));u=this._keyStr.indexOf(e.charAt(f++));a=this._keyStr.indexOf(e.charAt(f++));n=s<<2|o>>4;r=(o&15)<<4|u>>2;i=(u&3)<<6|a;t=t+String.fromCharCode(n);if(u!=64){t=t+String.fromCharCode(r)}if(a!=64){t=t+String.fromCharCode(i)}}t=Base64._utf8_decode(t);return t},_utf8_encode:function(e){e=e.replace(/\r\n/g,"\n");var t="";for(var n=0;n<e.length;n++){var r=e.charCodeAt(n);if(r<128){t+=String.fromCharCode(r)}else if(r>127&&r<2048){t+=String.fromCharCode(r>>6|192);t+=String.fromCharCode(r&63|128)}else{t+=String.fromCharCode(r>>12|224);t+=String.fromCharCode(r>>6&63|128);t+=String.fromCharCode(r&63|128)}}return t},_utf8_decode:function(e){var t="";var n=0;var r=c1=c2=0;while(n<e.length){r=e.charCodeAt(n);if(r<128){t+=String.fromCharCode(r);n++}else if(r>191&&r<224){c2=e.charCodeAt(n+1);t+=String.fromCharCode((r&31)<<6|c2&63);n+=2}else{c2=e.charCodeAt(n+1);c3=e.charCodeAt(n+2);t+=String.fromCharCode((r&15)<<12|(c2&63)<<6|c3&63);n+=3}}return t}};