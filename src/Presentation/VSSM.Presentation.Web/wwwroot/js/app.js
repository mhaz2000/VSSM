/*****
* CONFIGURATION
*/
//Main navigation
$.navigation = $('nav > ul.nav');

$.panelIconOpened = 'icon-arrow-up';
$.panelIconClosed = 'icon-arrow-down';

//Default colours
$.brandPrimary = '#20a8d8';
$.brandSuccess = '#4dbd74';
$.brandInfo = '#63c2de';
$.brandWarning = '#f8cb00';
$.brandDanger = '#f86c6b';

$.grayDark = '#2a2c36';
$.gray = '#55595c';
$.grayLight = '#818a91';
$.grayLighter = '#d1d4d7';
$.grayLightest = '#f8f9fa';

'use strict';

/****
* MAIN NAVIGATION
*/

$(document).ready(function ($) {

	$(function () {
		var currentUrl = window.location.pathname.toLowerCase();

		$('.sidebar .nav-link').each(function () {

			var linkUrl = $(this).attr('href');
			if (!linkUrl) return;

			linkUrl = linkUrl.toLowerCase();

			if (linkUrl === '/') {
				if (currentUrl === '/') {
					activateMenu($(this));
				}
				return;
			}

			if (currentUrl === linkUrl) {
				activateMenu($(this));
			}
		});

		function activateMenu($link) {

			// remove previous active (optional)
			$('.sidebar .nav-link.active').removeClass('active');

			$link.addClass('active');

			var $dropdown = $link.closest('.nav-dropdown');
			if ($dropdown.length) {
				$dropdown.addClass('open');
			}
		}
	});

	/* ---------------------------------
	   Sidebar Arrow Sync Function
	--------------------------------- */
	function updateMobileTogglerIcon() {

		var $icon = $('.mobile-toggler i.fa-angle-right, .mobile-toggler i.fa-angle-left');

		if ($('body').hasClass('mobile-open')) {
			$icon.removeClass('fa-angle-left')
				.addClass('fa-angle-right');
		} else {
			$icon.removeClass('fa-angle-right')
				.addClass('fa-angle-left');
		}
	}

	$('body').addClass('mobile-open');

	// Initial sync on page load
	updateMobileTogglerIcon();



	// Add class .active to current link
	$.navigation.find('a').each(function () {

		var cUrl = String(window.location);

		if (cUrl.substr(cUrl.length - 1) == '#') {
			cUrl = cUrl.slice(0, -1);
		}

	});


	// Dropdown Menu
	$.navigation.on('click', 'a', function (e) {

		if ($.ajaxLoad) {
			e.preventDefault();
		}

		if ($(this).hasClass('nav-dropdown-toggle')) {
			$(this).parent().toggleClass('open');
		}

		if (!$('body').hasClass('mobile-open')) {
			$('body').addClass('mobile-open');
			updateMobileTogglerIcon();
		}

	});


	function resizeBroadcast() {

		var timesRun = 0;
		var interval = setInterval(function () {
			timesRun += 1;
			if (timesRun === 5) {
				clearInterval(interval);
			}
			window.dispatchEvent(new Event('resize'));
		}, 62.5);
	}


	/* ---------- Main Menu Open/Close, Min/Full ---------- */
	$('.navbar-toggler').click(function () {

		var bodyClass = localStorage.getItem('body-class');

		if ($(this).hasClass('layout-toggler') && $('body').hasClass('sidebar-off-canvas')) {

			$('body').toggleClass('sidebar-opened').parent().toggleClass('sidebar-opened');

			$('.sidebar .nav-dropdown.open').removeClass('open');

			resizeBroadcast();

		}
		else if ($(this).hasClass('layout-toggler') && ($('body').hasClass('sidebar-nav') || bodyClass == 'sidebar-nav')) {

			$('body').toggleClass('sidebar-nav');
			localStorage.setItem('body-class', 'sidebar-nav');

			if (bodyClass == 'sidebar-nav') {
				localStorage.clear();
			}

			$('.sidebar .nav-dropdown.open').removeClass('open');

			resizeBroadcast();

		}
		else {

			// Mobile toggler case
			$('body').toggleClass('mobile-open');

			$('.sidebar .nav-dropdown.open').removeClass('open');

			updateMobileTogglerIcon();
		}
	});


	$('.aside-toggle').click(function () {
		$('body').toggleClass('aside-menu-open');

		resizeBroadcast();
	});


	$('.sidebar-close').click(function () {
		$('body').toggleClass('sidebar-opened')
			.parent().toggleClass('sidebar-opened');

		updateMobileTogglerIcon();
	});


	/* ---------- Disable moving to top ---------- */
	$('a[href="#"][data-top!=true]').click(function (e) {
		e.preventDefault();
	});

});


/****
* CARDS ACTIONS
*/

$(document).on('click', '.card-actions a', function (e) {
	e.preventDefault();

	if ($(this).hasClass('btn-close')) {
		$(this).parent().parent().parent().fadeOut();
	}
	else if ($(this).hasClass('btn-minimize')) {

		var $target = $(this).parent().parent().next('.card-block');

		if (!$(this).hasClass('collapsed')) {
			$('i', $(this)).removeClass($.panelIconOpened).addClass($.panelIconClosed);
		} else {
			$('i', $(this)).removeClass($.panelIconClosed).addClass($.panelIconOpened);
		}

	}
	else if ($(this).hasClass('btn-setting')) {
		$('#myModal').modal('show');
	}

});


function capitalizeFirstLetter(string) {
	return string.charAt(0).toUpperCase() + string.slice(1);
}


function init(url) {

	/* ---------- Tooltip ---------- */
	$('[rel="tooltip"],[data-rel="tooltip"]').tooltip({
		"placement": "bottom",
		delay: { show: 400, hide: 200 }
	});

	/* ---------- Popover ---------- */
	$('[rel="popover"],[data-rel="popover"],[data-toggle="popover"]').popover();

}
