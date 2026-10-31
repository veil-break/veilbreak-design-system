/* @veilbreak/gui — generated from com.veilbreak.gui by Tools~/web/package.py. Do not edit. */
import * as React from 'react';
import * as ReactDOM from 'react-dom';
const window = new Proxy({ React, ReactDOM, Veilbreak: {} }, { get: (t, k) => (k in t ? t[k] : globalThis[k]), set: (t, k, v) => { t[k] = v; return true; }, has: () => true });
/* The kit: a web port of the Unity package com.veilbreak.gui (UI Toolkit, portrait 390 x 844).
   Same element trees, BEM classes (the USS vb- names; the legacy kit above is renamed to vbl-)
   and Bind() logic as the C# sources; Gui.uss, Layout.uss and World.uss are ported mechanically into bundle.css. */
(function (window) {
  'use strict';
  var R = window.React;
  var h = R.createElement;
  // Layout effects run in the browser only, so server rendering (Next.js and the like) is quiet; the paint happens on hydration.
  var useIsoLayout = typeof document !== 'undefined' ? R.useLayoutEffect : R.useEffect;
  var ICONS = {"add":"<path d=\"M12 4v16 M4 12h16\"/>","armor":"<path d=\"M8 2h8l2 4 4 2-3 6-3-1v9H8v-9l-3 1-3-6 4-2Z\"/>","arrow":"<path d=\"M6 18L18 6M10 6H18V14\"/>","automation":"<path d=\"M19 6a8 8 0 1 0 1 10 M19 2v5h-5\"/>","autopilot":"<path d=\"M19 12A7 7 0 1 1 16.95 7.05M17 3V7.05H13\"/>","back":"<path d=\"M15 4 7 12l8 8\"/>","barrier":"<path d=\"M12 2 21 6v7l-9 9-9-9V6Z M7 11l4 4 6-7\"/>","bastion":"<path d=\"M3 4h18v10l-9 8-9-8Z M12 4v18\"/>","battle":"<path d=\"M4 3 20 19 M20 3 4 19 M3 15l6 6 M15 21l6-6 M4 3v5 M20 3v5\"/>","beam":"<path d=\"M2 8h20 M2 16h20 M5 12h14 M17 5l5 7-5 7\"/>","breaker":"<path d=\"M3 4h8v7H3Z M14 4h7v7h-7Z M3 14h8v7H3Z M17 14l4 7 M14 17l7-3\"/>","burst":"<g stroke-linecap=\"butt\" stroke-linejoin=\"miter\"><circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M5 15L15 5M7 18L18 7M10 20L20 10M4 11L11 4\"/></g>","burst-role":"<path d=\"M15.2 12A3.2 3.2 0 1 1 8.8 12A3.2 3.2 0 1 1 15.2 12M12 2V6M12 18V22M2 12H6M18 12H22M5 5L7.8 7.8M16.2 16.2L19 19M19 5L16.2 7.8M7.8 16.2L5 19\"/>","charm":"<path d=\"M12 4L20 12L12 20L4 12ZM12 9L15 12L12 15L9 12Z\"/>","check":"<path d=\"M3 12l6 6L21 5\"/>","chevron":"<path d=\"M8 4l8 8-8 8\"/>","chevron-left":"<path d=\"M15 6L9 12L15 18\"/>","circle":"<path d=\"M20 12A8 8 0 1 1 4 12A8 8 0 1 1 20 12\"/>","cleanse":"<path d=\"M12 2l3 7 7 3-7 3-3 7-3-7-7-3 7-3Z\"/>","close":"<path d=\"M5 5l14 14 M19 5 5 19\"/>","cone":"<path d=\"M12 21 2 5Q12-1 22 5Z\"/>","connection":"<path d=\"M2 7a15 15 0 0 1 20 0 M5 11a10 10 0 0 1 14 0 M8 15a5 5 0 0 1 8 0\"/><circle cx=\"12\" cy=\"19\" r=\"1\"/>","cooldown":"<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M12 5v7l5 3\"/>","core":"<path d=\"M12 3L20 7.5V16.5L12 21L4 16.5V7.5ZM14.6 12A2.6 2.6 0 1 1 9.4 12A2.6 2.6 0 1 1 14.6 12\"/>","counter":"<path d=\"M5 7h10a6 6 0 0 1 0 12h-4 M9 2 4 7l5 5\"/>","currency":"<path d=\"M12 2 21 7v10l-9 5-9-5V7Z M8 9l4-2 4 2v6l-4 2-4-2Z\"/>","dash":"<path d=\"M2 6h8 M2 12h12 M2 18h8 M13 3l9 9-9 9\"/>","debuffer":"<path d=\"M12 3L20 12L12 21L4 12ZM12 8V15M9 12L12 15L15 12\"/>","diamond":"<path d=\"M12 4L20 12L12 20L4 12Z\"/>","disruptor":"<path d=\"M13 3L6 13H12L10 21L18 10H12Z\"/>","exposed":"<path d=\"M12 2 21 6v7l-9 9-9-9V6Z M13 4 9 10l6 3-4 7\"/>","fan":"<path d=\"M12 21V3 M12 21 3 6 M12 21 21 6 M8 6l4-3 4 3\"/>","fullscreen":"<path d=\"M3 9V3h6 M15 3h6v6 M21 15v6h-6 M9 21H3v-6\"/>","haste":"<path d=\"M2 5l8 7-8 7 M12 5l8 7-8 7\"/>","healer":"<path d=\"M10 4H14V10H20V14H14V20H10V14H4V10H10Z\"/>","health":"<path d=\"M9 3h6v6h6v6h-6v6H9v-6H3V9h6Z\"/>","info":"<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M12 11v7 M12 6v1\"/>","interrupted":"<path d=\"M13 3L7 12H11.5M12.5 12H17L11 21\"/>","invalid":"<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M6 6l12 12\"/>","inventory":"<path d=\"M3 7l9-5 9 5v13H3Z M3 7h18 M8 11h8v5H8Z\"/>","key":"<circle cx=\"8\" cy=\"8\" r=\"5\"/><path d=\"M12 12l9 9 M16 16l3-3 M19 19l3-3\"/>","lock":"<path d=\"M5 10h14v11H5Z M8 10V6a4 4 0 0 1 8 0v4 M12 14v3\"/>","marker":"<path d=\"M12 2 22 12 12 22 2 12Z\"/><circle cx=\"12\" cy=\"12\" r=\"3\"/>","menu":"<path d=\"M4 7H20M4 12H20M4 17H20\"/>","next":"<path d=\"M6 6L14 12L6 18ZM18 6V18\"/>","no-target":"<path d=\"M18 12A6 6 0 1 1 6 12A6 6 0 1 1 18 12M12 3V8M12 16V21M3 12H8M16 12H21M4 4L20 20\"/>","offline":"<path d=\"M2 7a15 15 0 0 1 20 0 M5 11a10 10 0 0 1 14 0 M3 3l18 18\"/>","operator":"<circle cx=\"12\" cy=\"12\" r=\"4\"/><circle cx=\"4\" cy=\"4\" r=\"2\"/><circle cx=\"20\" cy=\"4\" r=\"2\"/><circle cx=\"12\" cy=\"21\" r=\"2\"/><path d=\"M6 6l3 3 M18 6l-3 3 M12 16v3\"/>","optic":"<path d=\"M2 12C5 7 8.5 5 12 5C15.5 5 19 7 22 12C19 17 15.5 19 12 19C8.5 19 5 17 2 12ZM14.6 12A2.6 2.6 0 1 1 9.4 12A2.6 2.6 0 1 1 14.6 12\"/>","pause":"<path d=\"M8 5v14 M16 5v14\"/>","pierce":"<path d=\"M4 12H20M15 7L20 12L15 17M4 8L7 12L4 16\"/>","plating":"<path d=\"M12 3L19 6V12C19 16 16 19 12 21C8 19 5 16 5 12V6ZM5 11H19\"/>","play":"<path d=\"M7 4v16l12-8z\"/>","power":"<path d=\"M14 2 5 14h6l-1 8 9-13h-6Z\"/>","prev":"<path d=\"M18 6L10 12L18 18ZM6 6V18\"/>","projectile":"<path d=\"M3 9h10V5l8 7-8 7v-4H3\"/>","protector":"<path d=\"M12 3L19 6V12C19 16 16 19 12 21C8 19 5 16 5 12V6Z\"/>","queue":"<path d=\"M3 4h18 M3 10h12 M3 16h8 M16 13l6 5-6 5\"/>","ranged":"<path d=\"M18 12A6 6 0 1 1 6 12A6 6 0 1 1 18 12M12 3V8M12 16V21M3 12H8M16 12H21\"/>","ranked":"<path d=\"M5 3h14v9l-7 5-7-5Z M12 17v4 M7 21h10 M2 4v5l3 2 M22 4v5l-3 2\"/>","remove":"<path d=\"m5 5 14 14 M19 5 5 19\"/>","replays":"<path d=\"M9 5v14l11-7Z M3 5v14\"/>","rifts":"<path d=\"M12 2 20 7 18 18 12 22 4 17 6 6Z M13 5 9 11 15 13 11 19\"/>","root":"<path d=\"M12 2v11 M5 7l7 6 7-6 M12 13l-7 8 M12 13l7 8 M12 13v9\"/>","set":"<path d=\"M3 3h7v7H3Z M14 3h7v7h-7Z M3 14h7v7H3Z M14 14h7v7h-7Z\"/>","settings":"<circle cx=\"12\" cy=\"12\" r=\"4\"/><path d=\"M12 1v3 M12 20v3 M1 12h3 M20 12h3 M4 4l2 2 M18 18l2 2 M4 20l2-2 M18 6l2-2\"/>","signal":"<path d=\"M5 19V16M10 19V12M15 19V8M20 19V4\"/>","silence":"<path d=\"M9 4h6v10H9Z M5 10v4a7 7 0 0 0 14 0v-4 M12 21v2 M2 2l20 20\"/>","skill-aegis-absolute-wall":"<path d=\"M4 3v18 M20 3v18 M4 6h16 M4 12h16 M4 18h16 M9 6v6 M15 12v6\"/>","skill-aegis-intercept-field":"<path d=\"M12 2 21 6v7l-9 9-9-9V6Z M7 11l4 4 6-7\"/>","skill-aria-needle-storm":"<path d=\"M19 5 5 12l14 7 M6 12h13\"/>","skill-aria-terminal-vector":"<path d=\"M3 8h18v8H3z M7 9l-3 6 M13 9l-4 6 M19 9l-4 6\"/>","skill-echo-clocksplice":"<path d=\"M2 5l8 7-8 7 M12 5l8 7-8 7\"/>","skill-echo-overclock":"<circle cx=\"12\" cy=\"12\" r=\"9\"/><circle cx=\"12\" cy=\"12\" r=\"5\"/><circle cx=\"12\" cy=\"12\" r=\"1\"/>","skill-hex-black-signal":"<path d=\"M12 2 22 12 12 22 2 12Z\"/><circle cx=\"12\" cy=\"12\" r=\"3\"/>","skill-hex-contagion":"<path d=\"M12 1v5 M12 18v5 M1 12h5 M18 12h5 M4 4l4 4 M16 16l4 4 M4 20l4-4 M16 8l4-4\"/><circle cx=\"12\" cy=\"12\" r=\"3\"/>","skill-mira-counterdrive":"<path d=\"M2 6h8 M2 12h12 M2 18h8 M13 3l9 9-9 9\"/>","skill-mira-gravity-cage":"<circle cx=\"12\" cy=\"12\" r=\"9\"/><circle cx=\"12\" cy=\"12\" r=\"5\"/><circle cx=\"12\" cy=\"12\" r=\"1\"/>","skill-nyra-reconstruct":"<path d=\"M9 3h6v6h6v6h-6v6H9v-6H3V9h6Z\"/>","skill-nyra-white-noise":"<path d=\"M12 2l3 7 7 3-7 3-3 7-3-7-7-3 7-3Z\"/>","skill-rook-crash-pulse":"<path d=\"M12 21 2 5Q12-1 22 5Z\"/>","skill-rook-system-seizure":"<circle cx=\"12\" cy=\"12\" r=\"9\"/><circle cx=\"12\" cy=\"12\" r=\"5\"/><circle cx=\"12\" cy=\"12\" r=\"1\"/>","skill-sol-dead-horizon":"<path d=\"M2 8h20 M2 16h20 M5 12h14 M17 5l5 7-5 7\"/>","skill-sol-zero-line":"<path d=\"M3 9h10V5l8 7-8 7v-4H3\"/>","skirmisher":"<path d=\"M5 6L11 12L5 18M12 6L18 12L12 18\"/>","slow":"<path d=\"M3 7l6 5-6 5 M10 7l6 5-6 5 M21 3v18\"/>","striker":"<circle cx=\"12\" cy=\"12\" r=\"4\"/><path d=\"M12 1v3 M12 20v3 M1 12h3 M20 12h3 M4 4l2 2 M18 18l2 2 M4 20l2-2 M18 6l2-2\"/>","studio":"<path d=\"M5 20 3 15 15 3 21 9 9 21Z M12 6l6 6 M3 15l6 6\"/>","support":"<path d=\"M3 12H7L9.5 6L14 18L16 12H21\"/>","surrender":"<path d=\"M6 21V4M6 4H18L15 8.5L18 13H6\"/>","target":"<circle cx=\"12\" cy=\"12\" r=\"6\"/><path d=\"M12 1v7 M12 16v7 M1 12h7 M16 12h7\"/>","team":"<circle cx=\"8\" cy=\"7\" r=\"3\"/><circle cx=\"17\" cy=\"9\" r=\"2\"/><path d=\"M2 21v-4a6 6 0 0 1 12 0v4 M16 15h2a4 4 0 0 1 4 4v2\"/>","training":"<circle cx=\"12\" cy=\"12\" r=\"7\"/><circle cx=\"12\" cy=\"12\" r=\"2\"/><path d=\"M12 1v4 M12 19v4 M1 12h4 M19 12h4\"/>","volume":"<path d=\"M3 9h5l6-6v18l-6-6H3Z M18 7a8 8 0 0 1 0 10\"/>","wall":"<path d=\"M4 3v18 M20 3v18 M4 6h16 M4 12h16 M4 18h16 M9 6v6 M15 12v6\"/>","warning":"<path d=\"M12 2 23 21H1Z M12 8v6 M12 17v1\"/>","weapon":"<path d=\"M4 20L16 8M13 4H20V11M7 13L11 17\"/>","windup":"<path d=\"M6 2h12 M6 22h12 M7 2v5l10 10v5 M17 2v5L7 17v5\"/>","zone":"<circle cx=\"12\" cy=\"12\" r=\"9\"/><circle cx=\"12\" cy=\"12\" r=\"5\"/><circle cx=\"12\" cy=\"12\" r=\"1\"/>","plus":"<path d=\"M12 4v16 M4 12h16\"/>","silenced":"<path d=\"M9 4h6v10H9Z M5 10v4a7 7 0 0 0 14 0v-4 M12 21v2 M2 2l20 20\"/>","rooted":"<path d=\"M12 2v11 M5 7l7 6 7-6 M12 13l-7 8 M12 13l7 8 M12 13v9\"/>","slowed":"<path d=\"M3 7l6 5-6 5 M10 7l6 5-6 5 M21 3v18\"/>","replay":"<path d=\"M9 5v14l11-7Z M3 5v14\"/>","cancel":"<path d=\"M5 5l14 14 M19 5 5 19\"/>","chevron-right":"<path d=\"M8 4l8 8-8 8\"/>","rift":"<path d=\"M12 2 20 7 18 18 12 22 4 17 6 6Z M13 5 9 11 15 13 11 19\"/>"};
  var MARK = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"64 245 513 259\" width=\"513\" height=\"259\"> <g transform=\"translate(-2.5 0)\" fill=\"#ff40a0\" fill-opacity=\".55\"> <g> <g> <path d=\"M172.445 491.288 L210.995 421.197 L210.995 428.692 Z\"/> <path d=\"M231.395 401.288 L231.395 364.8 L249.539 364.8 C250.183 364.8 250.81 364.826 251.421 364.877 L251.421 364.877 L248.69 369.843 C248.589 369.841 248.488 369.84 248.387 369.84 L237.227 369.84 L237.227 386.4 L239.583 386.4 Z\"/> <path d=\"M510.899 410.216 L510.899 359.816 L516.731 359.816 L516.731 375.512 L516.731 386.384 L516.875 386.384 L525.083 377.672 L541.859 359.816 L548.915 359.816 L529.979 379.976 L550.859 410.288 L543.947 410.216 L525.659 384.152 L516.731 393.44 L516.731 410.216 Z\"/> <path d=\"M432.707 410.216 L454.163 359.816 L460.571 359.816 L482.027 410.216 L475.763 410.216 L470.147 396.824 L444.587 396.824 L438.971 410.216 Z M457.259 366.512 C457.187 366.584 454.235 373.712 452.363 378.248 L446.819 391.496 L467.915 391.496 L462.371 378.32 C460.427 373.712 457.475 366.584 457.403 366.512 Z\"/> <path d=\"M371.795 410.216 L371.795 359.816 L405.707 359.816 L405.707 365.288 L377.627 365.288 L377.627 381.128 L399.515 381.128 L399.515 386.6 L377.627 386.6 L377.627 404.744 L405.851 404.744 L405.851 410.216 Z\"/> <path d=\"M306.851 410.216 L306.851 359.816 L322.259 359.816 C334.067 359.816 341.051 365.648 341.051 375.512 C341.051 383.288 336.659 387.824 329.531 389.336 L341.987 410.216 L334.931 410.216 L323.483 390.416 C322.403 390.488 319.955 390.488 318.875 390.488 L312.683 390.488 L312.683 410.216 Z M312.683 365.216 L312.683 385.592 L322.835 385.592 C330.179 385.592 335.075 383 335.075 375.728 C335.075 368.6 330.323 365.216 322.475 365.216 Z\"/> <path d=\"M262.98 365.899 L265.743 361.112 C270.686 363.107 273.587 367.224 273.587 372.2 C273.587 378.824 269.339 382.208 265.667 383.072 L265.667 383.288 C270.419 384.08 277.115 387.608 277.115 396.176 C277.115 404.672 270.995 410.216 259.907 410.216 L240.395 410.216 L240.395 403.799 L246.227 394.329 L246.227 405.176 L259.619 405.176 C266.387 405.176 270.995 402.08 270.995 395.888 C270.995 389.552 265.667 386.024 259.475 386.024 L251.341 386.024 L251.729 385.395 L254.025 381.416 L257.963 381.416 C263.579 381.416 267.683 378.536 267.683 373.064 C267.683 369.768 266.003 367.233 262.98 365.899 Z\"/> <path d=\"M228.29 389.752 L231.021 384.786 C232.985 384.95 234.768 385.37 236.344 386.006 L233.581 390.793 L224.626 406.309 L222.329 410.288 L221.942 410.917 L216.827 419.222 L210.995 428.692 L210.995 421.197 L219.184 406.309 Z\"/> <path d=\"M274.439 322.579 L307.445 279.288 L256.865 366.928 L250.047 366.928 Z\"/> <path d=\"M240.704 383.965 L253.726 360.288 L260.395 360.288 L246.026 385.184 C244.45 384.549 242.667 384.128 240.704 383.965 Z\"/> <path d=\"M271.302 322.76 L274.311 317.288 L281.129 317.288 L298.445 317.288 L298.445 322.76 L277.971 322.76 Z\"/> <g> <path d=\"M267.773 320.4 L267.773 270 L273.605 270 L273.605 314.928 L276.039 314.928 L273.03 320.4 Z\"/> <path d=\"M229.253 320.4 L229.253 270 L235.085 270 L235.085 320.4 Z\"/> <path d=\"M164.453 320.4 L164.453 270 L198.365 270 L198.365 275.472 L170.285 275.472 L170.285 291.312 L192.173 291.312 L192.173 296.784 L170.285 296.784 L170.285 314.928 L198.509 314.928 L198.509 320.4 Z\"/> <path d=\"M109.157 320.4 L89.141 270 L95.693 270 L105.989 296.424 C110.093 306.936 112.613 313.632 112.685 313.848 L112.829 313.848 C112.901 313.632 115.349 307.008 119.381 296.784 L129.821 270 L136.157 270 L116.141 320.4 Z\"/> </g> </g> </g> </g> <g transform=\"translate(2.5 0)\" fill=\"#1dd9a0\" fill-opacity=\".55\"> <g> <g> <path d=\"M172.445 491.288 L210.995 421.197 L210.995 428.692 Z\"/> <path d=\"M231.395 401.288 L231.395 364.8 L249.539 364.8 C250.183 364.8 250.81 364.826 251.421 364.877 L251.421 364.877 L248.69 369.843 C248.589 369.841 248.488 369.84 248.387 369.84 L237.227 369.84 L237.227 386.4 L239.583 386.4 Z\"/> <path d=\"M510.899 410.216 L510.899 359.816 L516.731 359.816 L516.731 375.512 L516.731 386.384 L516.875 386.384 L525.083 377.672 L541.859 359.816 L548.915 359.816 L529.979 379.976 L550.859 410.288 L543.947 410.216 L525.659 384.152 L516.731 393.44 L516.731 410.216 Z\"/> <path d=\"M432.707 410.216 L454.163 359.816 L460.571 359.816 L482.027 410.216 L475.763 410.216 L470.147 396.824 L444.587 396.824 L438.971 410.216 Z M457.259 366.512 C457.187 366.584 454.235 373.712 452.363 378.248 L446.819 391.496 L467.915 391.496 L462.371 378.32 C460.427 373.712 457.475 366.584 457.403 366.512 Z\"/> <path d=\"M371.795 410.216 L371.795 359.816 L405.707 359.816 L405.707 365.288 L377.627 365.288 L377.627 381.128 L399.515 381.128 L399.515 386.6 L377.627 386.6 L377.627 404.744 L405.851 404.744 L405.851 410.216 Z\"/> <path d=\"M306.851 410.216 L306.851 359.816 L322.259 359.816 C334.067 359.816 341.051 365.648 341.051 375.512 C341.051 383.288 336.659 387.824 329.531 389.336 L341.987 410.216 L334.931 410.216 L323.483 390.416 C322.403 390.488 319.955 390.488 318.875 390.488 L312.683 390.488 L312.683 410.216 Z M312.683 365.216 L312.683 385.592 L322.835 385.592 C330.179 385.592 335.075 383 335.075 375.728 C335.075 368.6 330.323 365.216 322.475 365.216 Z\"/> <path d=\"M262.98 365.899 L265.743 361.112 C270.686 363.107 273.587 367.224 273.587 372.2 C273.587 378.824 269.339 382.208 265.667 383.072 L265.667 383.288 C270.419 384.08 277.115 387.608 277.115 396.176 C277.115 404.672 270.995 410.216 259.907 410.216 L240.395 410.216 L240.395 403.799 L246.227 394.329 L246.227 405.176 L259.619 405.176 C266.387 405.176 270.995 402.08 270.995 395.888 C270.995 389.552 265.667 386.024 259.475 386.024 L251.341 386.024 L251.729 385.395 L254.025 381.416 L257.963 381.416 C263.579 381.416 267.683 378.536 267.683 373.064 C267.683 369.768 266.003 367.233 262.98 365.899 Z\"/> <path d=\"M228.29 389.752 L231.021 384.786 C232.985 384.95 234.768 385.37 236.344 386.006 L233.581 390.793 L224.626 406.309 L222.329 410.288 L221.942 410.917 L216.827 419.222 L210.995 428.692 L210.995 421.197 L219.184 406.309 Z\"/> <path d=\"M274.439 322.579 L307.445 279.288 L256.865 366.928 L250.047 366.928 Z\"/> <path d=\"M240.704 383.965 L253.726 360.288 L260.395 360.288 L246.026 385.184 C244.45 384.549 242.667 384.128 240.704 383.965 Z\"/> <path d=\"M271.302 322.76 L274.311 317.288 L281.129 317.288 L298.445 317.288 L298.445 322.76 L277.971 322.76 Z\"/> <g> <path d=\"M267.773 320.4 L267.773 270 L273.605 270 L273.605 314.928 L276.039 314.928 L273.03 320.4 Z\"/> <path d=\"M229.253 320.4 L229.253 270 L235.085 270 L235.085 320.4 Z\"/> <path d=\"M164.453 320.4 L164.453 270 L198.365 270 L198.365 275.472 L170.285 275.472 L170.285 291.312 L192.173 291.312 L192.173 296.784 L170.285 296.784 L170.285 314.928 L198.509 314.928 L198.509 320.4 Z\"/> <path d=\"M109.157 320.4 L89.141 270 L95.693 270 L105.989 296.424 C110.093 306.936 112.613 313.632 112.685 313.848 L112.829 313.848 C112.901 313.632 115.349 307.008 119.381 296.784 L129.821 270 L136.157 270 L116.141 320.4 Z\"/> </g> </g> </g> </g> <g fill=\"#faeef4\"> <g> <g> <path d=\"M172.445 491.288 L210.995 421.197 L210.995 428.692 Z\"/> <path d=\"M231.395 401.288 L231.395 364.8 L249.539 364.8 C250.183 364.8 250.81 364.826 251.421 364.877 L251.421 364.877 L248.69 369.843 C248.589 369.841 248.488 369.84 248.387 369.84 L237.227 369.84 L237.227 386.4 L239.583 386.4 Z\"/> <path d=\"M510.899 410.216 L510.899 359.816 L516.731 359.816 L516.731 375.512 L516.731 386.384 L516.875 386.384 L525.083 377.672 L541.859 359.816 L548.915 359.816 L529.979 379.976 L550.859 410.288 L543.947 410.216 L525.659 384.152 L516.731 393.44 L516.731 410.216 Z\"/> <path d=\"M432.707 410.216 L454.163 359.816 L460.571 359.816 L482.027 410.216 L475.763 410.216 L470.147 396.824 L444.587 396.824 L438.971 410.216 Z M457.259 366.512 C457.187 366.584 454.235 373.712 452.363 378.248 L446.819 391.496 L467.915 391.496 L462.371 378.32 C460.427 373.712 457.475 366.584 457.403 366.512 Z\"/> <path d=\"M371.795 410.216 L371.795 359.816 L405.707 359.816 L405.707 365.288 L377.627 365.288 L377.627 381.128 L399.515 381.128 L399.515 386.6 L377.627 386.6 L377.627 404.744 L405.851 404.744 L405.851 410.216 Z\"/> <path d=\"M306.851 410.216 L306.851 359.816 L322.259 359.816 C334.067 359.816 341.051 365.648 341.051 375.512 C341.051 383.288 336.659 387.824 329.531 389.336 L341.987 410.216 L334.931 410.216 L323.483 390.416 C322.403 390.488 319.955 390.488 318.875 390.488 L312.683 390.488 L312.683 410.216 Z M312.683 365.216 L312.683 385.592 L322.835 385.592 C330.179 385.592 335.075 383 335.075 375.728 C335.075 368.6 330.323 365.216 322.475 365.216 Z\"/> <path d=\"M262.98 365.899 L265.743 361.112 C270.686 363.107 273.587 367.224 273.587 372.2 C273.587 378.824 269.339 382.208 265.667 383.072 L265.667 383.288 C270.419 384.08 277.115 387.608 277.115 396.176 C277.115 404.672 270.995 410.216 259.907 410.216 L240.395 410.216 L240.395 403.799 L246.227 394.329 L246.227 405.176 L259.619 405.176 C266.387 405.176 270.995 402.08 270.995 395.888 C270.995 389.552 265.667 386.024 259.475 386.024 L251.341 386.024 L251.729 385.395 L254.025 381.416 L257.963 381.416 C263.579 381.416 267.683 378.536 267.683 373.064 C267.683 369.768 266.003 367.233 262.98 365.899 Z\"/> <path d=\"M228.29 389.752 L231.021 384.786 C232.985 384.95 234.768 385.37 236.344 386.006 L233.581 390.793 L224.626 406.309 L222.329 410.288 L221.942 410.917 L216.827 419.222 L210.995 428.692 L210.995 421.197 L219.184 406.309 Z\"/> <path d=\"M274.439 322.579 L307.445 279.288 L256.865 366.928 L250.047 366.928 Z\"/> <path d=\"M240.704 383.965 L253.726 360.288 L260.395 360.288 L246.026 385.184 C244.45 384.549 242.667 384.128 240.704 383.965 Z\"/> <path d=\"M271.302 322.76 L274.311 317.288 L281.129 317.288 L298.445 317.288 L298.445 322.76 L277.971 322.76 Z\"/> <g> <path d=\"M267.773 320.4 L267.773 270 L273.605 270 L273.605 314.928 L276.039 314.928 L273.03 320.4 Z\"/> <path d=\"M229.253 320.4 L229.253 270 L235.085 270 L235.085 320.4 Z\"/> <path d=\"M164.453 320.4 L164.453 270 L198.365 270 L198.365 275.472 L170.285 275.472 L170.285 291.312 L192.173 291.312 L192.173 296.784 L170.285 296.784 L170.285 314.928 L198.509 314.928 L198.509 320.4 Z\"/> <path d=\"M109.157 320.4 L89.141 270 L95.693 270 L105.989 296.424 C110.093 306.936 112.613 313.632 112.685 313.848 L112.829 313.848 C112.901 313.632 115.349 307.008 119.381 296.784 L129.821 270 L136.157 270 L116.141 320.4 Z\"/> </g> </g> </g> </g> </svg>";

  function cx() { return Array.prototype.filter.call(arguments, Boolean).join(' '); }
  function clamp01(n) { n = Number(n); return isNaN(n) ? 0 : Math.max(0, Math.min(1, n)); }
  function pct(n) { return (clamp01(n) * 100) + '%'; }
  function kebab(n) {
    n = String(n == null ? '' : n); var b = '';
    for (var i = 0; i < n.length; i++) {
      var c = n[i];
      if (c >= 'A' && c <= 'Z' && i > 0 && n[i - 1] !== '-') b += '-';
      b += (c === ' ' || c === '_') ? '-' : c.toLowerCase();
    }
    return b;
  }
  function upper(s) { return String(s == null ? '' : s).toUpperCase(); }
  function num(v) { return Number(v || 0).toLocaleString('en-US'); }
  function trim1(v) { return String(Math.round(v * 10) / 10); }
  function compact(v) { return v >= 1e9 ? trim1(v / 1e9) + 'B' : v >= 1e6 ? trim1(v / 1e6) + 'M' : v >= 1e4 ? trim1(v / 1e3) + 'K' : num(v); }
  function seconds(s) {
    s = Number(s || 0);
    if (s < 60) return (s < 10 ? s.toFixed(1) : trim1(s)) + 's';
    var m = Math.floor(s / 60), r = Math.floor(s - m * 60);
    return m + ':' + (r < 10 ? '0' : '') + r;
  }
  function pad2(n) { n = Math.floor(Number(n || 0)); return (n < 10 && n >= 0 ? '0' : '') + n; }
  // Ui.State: block--{kebab(state)} for the one active value.
  function st(block, value) { return block + '--' + kebab(value); }
  function L(cls, text, style) { return h('div', { className: cx('vb-l', cls), style: style }, text); }
  function show(on) { return on ? null : { display: 'none' }; }
  function clickable(fn) {
    return {
      tabIndex: 0, role: 'button', onClick: fn,
      onKeyDown: function (e) { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); if (fn) fn(e); } }
    };
  }

  /* ── VbShape: the cut-shape painter (VbShape.cs Paint) drawn as SVG from --vb-* properties ── */
  var DIAG = 0.4142, shapes = typeof Set === 'function' ? new Set() : null, queued = false, installed = false;
  function parseColor(s) {
    s = (s || '').trim(); var m;
    if (!s || s === 'transparent') return [0, 0, 0, 0];
    if (s[0] === '#') {
      var x = s.slice(1); if (x.length <= 4) x = x.split('').map(function (c) { return c + c; }).join('');
      return [parseInt(x.slice(0, 2), 16), parseInt(x.slice(2, 4), 16), parseInt(x.slice(4, 6), 16), x.length > 6 ? parseInt(x.slice(6, 8), 16) / 255 : 1];
    }
    if ((m = /rgba?\(([^)]*)\)/.exec(s))) {
      var p = m[1].split(/[\s,\/]+/).filter(Boolean).map(parseFloat);
      return [p[0], p[1], p[2], p.length > 3 ? p[3] : 1];
    }
    return [0, 0, 0, 0];
  }
  function rgba(c, a) { return 'rgba(' + c[0] + ',' + c[1] + ',' + c[2] + ',' + (a == null ? c[3] : a) + ')'; }
  function outline(w, hgt, cut, b) {
    if (cut <= 0) return [[b, b], [w - b, b], [w - b, hgt - b], [b, hgt - b]];
    var d = cut + b * DIAG;
    return [[d, b], [w - b, b], [w - b, hgt - d], [w - d, hgt - b], [b, hgt - b], [b, d]];
  }
  function poly(pts) { return 'M' + pts.map(function (p) { return p[0].toFixed(2) + ' ' + p[1].toFixed(2); }).join('L') + 'Z'; }
  function rect(x0, y0, x1, y1, fill) { return '<rect x="' + x0 + '" y="' + y0 + '" width="' + Math.max(0, x1 - x0) + '" height="' + Math.max(0, y1 - y0) + '" fill="' + fill + '"/>'; }
  function gradient(id, w, hgt, deg, from, to) {
    var r = deg * Math.PI / 180, dx = Math.cos(r), dy = Math.sin(r);
    var pr = [0, w * dx, hgt * dy, w * dx + hgt * dy], min = Math.min.apply(null, pr), max = Math.max.apply(null, pr);
    if (max - min < 0.001) max = min + 0.001;
    return '<linearGradient id="' + id + '" gradientUnits="userSpaceOnUse" x1="' + dx * min + '" y1="' + dy * min + '" x2="' + dx * max + '" y2="' + dy * max + '">' +
      '<stop offset="0" stop-color="' + rgba(from) + '"/><stop offset="1" stop-color="' + rgba(to) + '"/></linearGradient>';
  }
  var gid = 0;
  function paint(el) {
    var svg = el.firstElementChild;
    if (!svg || svg.getAttribute('data-vb-paint') == null) return;
    var w = el.offsetWidth, hgt = el.offsetHeight;
    if (w <= 0 || hgt <= 0) { svg.innerHTML = ''; return; }
    var cs = getComputedStyle(el);
    function n(k, d) { var v = parseFloat(cs.getPropertyValue('--vb-' + k)); return isNaN(v) ? d : v; }
    function c(k) { return parseColor(cs.getPropertyValue('--vb-' + k)); }
    var cut = Math.min(n('cut', 10), w / 3, hgt / 3), bw = n('bw', 1), dir = n('fill-dir', 0), dash = n('dash', 0) > 0.5, bl = n('bracket-len', 8);
    var fill = c('fill'), fillTo = c('fill-to'), edge = c('edge'), edgeTo = c('edge-to'), bracket = c('bracket'), glow = c('glow');
    var id = svg.__vbId || (svg.__vbId = 'vb-g' + (++gid)), defs = '', out = '';
    function paintOf(from, to, key) {
      if (to[3] <= 0 || (to[0] === from[0] && to[1] === from[1] && to[2] === from[2] && to[3] === from[3])) return rgba(from);
      defs += gradient(id + key, w, hgt, dir, from, to); return 'url(#' + id + key + ')';
    }
    if (glow[3] > 0) for (var i = 3; i >= 1; i--) {
      out += '<path fill-rule="evenodd" fill="' + rgba(glow, glow[3] * (0.55 - i * 0.15)) + '" d="' + poly(outline(w, hgt, cut, -i * 2.5)) + poly(outline(w, hgt, cut, -(i - 1) * 2.5)) + '"/>';
    }
    if (dash) {
      if (fill[3] > 0) out += rect(0, 0, w, hgt, rgba(fill));
      if (edge[3] > 0) {
        var t = Math.max(1, bw), e, x, y, col = rgba(edge);
        for (x = 0; x < w; x += 7) { e = Math.min(w, x + 4); out += rect(x, 0, e, t, col) + rect(x, hgt - t, e, hgt, col); }
        for (y = 0; y < hgt; y += 7) { e = Math.min(hgt, y + 4); out += rect(0, y, t, e, col) + rect(w - t, y, w, e, col); }
      }
    } else {
      if (fill[3] > 0 || fillTo[3] > 0) out += '<path fill="' + paintOf(fill[3] > 0 ? fill : fillTo, fillTo, 'f') + '" d="' + poly(outline(w, hgt, cut, 0)) + '"/>';
      if (bw > 0 && (edge[3] > 0 || edgeTo[3] > 0)) out += '<path fill-rule="evenodd" fill="' + paintOf(edge[3] > 0 ? edge : edgeTo, edgeTo, 'e') + '" d="' + poly(outline(w, hgt, cut, 0)) + poly(outline(w, hgt, cut, bw)) + '"/>';
    }
    if (bracket[3] > 0) {
      var bc = rgba(bracket);
      out += rect(w - bl, 0, w, 2, bc) + rect(w - 2, 0, w, bl, bc) + rect(0, hgt - 2, bl, hgt, bc) + rect(0, hgt - bl, 2, hgt, bc);
    }
    svg.setAttribute('viewBox', '-8 -8 ' + (w + 16) + ' ' + (hgt + 16));
    svg.innerHTML = (defs ? '<defs>' + defs + '</defs>' : '') + out;
  }
  function repaintAll() { queued = false; if (shapes) shapes.forEach(paint); }
  function schedule() { if (!queued) { queued = true; requestAnimationFrame(repaintAll); } }
  function install() {
    if (installed) return; installed = true;
    ['pointerover', 'pointerout', 'pointerdown', 'pointerup', 'focusin', 'focusout', 'transitionend'].forEach(function (t) { document.addEventListener(t, schedule, true); });
    window.addEventListener('resize', schedule);
    if (window.MutationObserver) new MutationObserver(schedule).observe(document.documentElement, { attributes: true, attributeFilter: ['class', 'style'], subtree: true });
    if (document.fonts && document.fonts.ready) document.fonts.ready.then(schedule);
  }
  var ro = window.ResizeObserver ? new ResizeObserver(schedule) : null;
  function useShape(ref) {
    useIsoLayout(function () {
      var el = ref.current; install(); shapes.add(el); if (ro) ro.observe(el); paint(el);
      return function () { shapes.delete(el); if (ro) ro.unobserve(el); };
    }, []);
    useIsoLayout(function () { schedule(); });
  }
  /** Any element painted as a cut shape (VbShape.Attach): first child is the paint layer. */
  var Shape = R.forwardRef(function (p, fwd) {
    var ref = R.useRef(null);
    useShape(ref);
    var props = Object.assign({}, p, {
      ref: function (el) { ref.current = el; if (typeof fwd === 'function') fwd(el); else if (fwd) fwd.current = el; },
      className: cx('vb-shape', p.className)
    });
    delete props.as;
    return h(p.as || 'div', props, h('svg', { className: 'vb-paint', 'data-vb-paint': '', 'aria-hidden': true }), p.children);
  });

  var ROOT_FLAGS = ['vb-compact', 'vb-large-text', 'vb-reduced-motion', 'vb-reduced-flashes', 'vb-contrast'];  // SafeAreaRoot.RootClasses
  /* ── Popover (Popover.cs): hover on a mouse, tap toggles on touch, focus opens; kept inside the viewport ── */
  function usePopover(build, placement, alignEnd, pinned) {
    var anchor = R.useRef(null), pop = R.useRef(null);
    var s = R.useState(false), open = s[0] || !!pinned, setOpen = s[1];
    var pos = R.useState(null), at = pos[0], setAt = pos[1], tick = R.useState(0)[1];
    // Popover.Place runs on every geometry change; re-place on resize, scroll and anchor resize.
    R.useEffect(function () {
      if (!open) return;
      function again() { tick(function (n) { return n + 1; }); }
      window.addEventListener('resize', again); window.addEventListener('scroll', again, true);
      var obs = window.ResizeObserver && anchor.current ? new ResizeObserver(again) : null; if (obs) obs.observe(anchor.current);
      return function () { window.removeEventListener('resize', again); window.removeEventListener('scroll', again, true); if (obs) obs.disconnect(); };
    }, [open]);
    useIsoLayout(function () {
      if (!open || !anchor.current || !pop.current) return;
      var a = anchor.current.getBoundingClientRect(), wv = window.innerWidth, hv = window.innerHeight;
      var w = pop.current.offsetWidth, hh = pop.current.offsetHeight, gap = 6, edge = 8;
      var x = alignEnd ? a.right - w : a.left, y = placement === 'above' ? a.top - gap - hh : a.bottom + gap;
      if (placement !== 'above' && y + hh > hv - edge) y = a.top - gap - hh;
      if (placement === 'above' && y < edge) y = a.bottom + gap;
      x = Math.min(Math.max(x, edge), Math.max(edge, wv - w - edge)); y = Math.min(Math.max(y, edge), Math.max(edge, hv - hh - edge));
      if (!at || Math.abs(at[0] - x) > 0.5 || Math.abs(at[1] - y) > 0.5) setAt([x, y]);
    });
    var handlers = {
      onPointerEnter: function (e) { if (e.pointerType === 'mouse') setOpen(true); },
      onPointerLeave: function (e) { if (e.pointerType === 'mouse') setOpen(false); },
      onPointerDown: function (e) { if (e.pointerType !== 'mouse') setOpen(function (o) { return !o; }); },
      onFocus: function () { setOpen(true); }, onBlur: function () { setOpen(false); }
    };
    var content = open ? build() : null, a = content && anchor.current;
    // Popover.Open: the layer sits outside the board, so it copies the anchor's root flags and theme.
    var flags = a ? ROOT_FLAGS.filter(function (c) { return a.closest('.' + c); }) : [], themed = a && a.closest('[data-theme]');
    var layer = content ? window.ReactDOM.createPortal(h('div', {
      ref: pop, className: cx('vb-root vb-popover', flags.join(' ')), 'data-theme': themed ? themed.getAttribute('data-theme') : undefined,
      style: { position: 'fixed', left: at ? at[0] : -9999, top: at ? at[1] : -9999, maxWidth: Math.max(80, window.innerWidth - 16), maxHeight: Math.max(80, window.innerHeight - 32), overflowY: 'auto', zIndex: 50, pointerEvents: 'none' }
    }, content), document.body) : null;
    return { ref: anchor, open: !!content, handlers: handlers, layer: layer };
  }

  /* ── Controls ─────────────────────────────────────────────────────────── */
  function VbIcon(p) {
    var name = kebab(p.icon || ''), size = p.size == null ? 16 : p.size;
    var style = Object.assign({ width: size, height: size }, p.style || {}, name ? null : { display: 'none' });
    return h('div', { className: cx('vb-icon', name && 'vb-icon--' + name, p.className), style: style, title: p.title },
      ICONS[name] ? h('svg', { viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', strokeWidth: 1.6, strokeLinecap: 'square', strokeLinejoin: 'miter', width: '100%', height: '100%', 'aria-hidden': true, dangerouslySetInnerHTML: { __html: ICONS[name] } }) : null);
  }
  VbIcon.names = Object.keys(ICONS);

  function VbMark(p) {
    var w = p.width == null ? 260 : p.width;
    return h('div', { className: cx('vb-mark', p.className), role: 'img', 'aria-label': 'Veilbreak', style: Object.assign({ width: w, height: Math.round(w * 259 / 513) }, p.style), dangerouslySetInnerHTML: { __html: MARK.replace('<svg ', '<svg width="100%" height="100%" ') } });
  }

  function VbFrame(p) { return h(Shape, { className: cx(frameClass(p.kind), p.className), style: p.style }, p.children); }

  function VbButton(p) {
    var variant = p.variant || 'secondary', blocked = p.blocked || '', sub = blocked || p.sub || '', icon = p.icon || '';
    return h(Shape, {
      as: 'button', type: 'button', disabled: !!blocked, onClick: blocked ? null : p.onClick, style: p.style,
      className: cx('vb-button', st('vb-button', variant), blocked && 'vb-button--blocked', p.className)
    },
      h('div', { className: 'vb-button__text' }, L('vb-button__label', p.title || ''), L('vb-button__sub', sub, show(sub.length > 0))),
      h(VbIcon, { icon: icon, size: icon === 'chevron' || icon === 'play' ? 12 : 20, className: 'vb-button__icon' }));
  }

  // StatReadout.Delta: a leading + reads up (teal), − down (pink); `up` overrides when lower is better.
  function StatReadout(p) {
    var bar = p.bar == null ? -1 : p.bar, d = p.delta || '', c = d.charAt(0);
    var rise = p.up != null ? !!p.up : c === '+', fall = p.up != null ? !p.up : c === '−' || c === '-';
    return h('div', { className: cx('vb-stat', p.tone && 'vb-stat--' + p.tone, p.className) },
      L('vb-stat__label', p.label),
      h('div', { className: 'vb-stat__row' }, L('vb-stat__value', p.value), L('vb-stat__unit', p.unit || '', show(!!p.unit)),
        L(cx('vb-stat__delta', d && rise && 'vb-stat__delta--up', d && fall && !rise && 'vb-stat__delta--down'), d, show(d.length > 0))),
      bar >= 0 ? h(ProgressBar, { value: bar, className: 'vb-stat__bar' }) : null);
  }

  var STATUS_ICON = { exposed: 'exposed', silenced: 'silence', rooted: 'root', slowed: 'slow', interrupted: 'warning', barrier: 'barrier' };
  function statusIcon(kind) { return STATUS_ICON[kebab(kind)] || 'barrier'; }
  function StatusChip(p) {
    var kind = kebab(p.kind || 'barrier');
    return h(Shape, { className: cx('vb-status', st('vb-status', kind), p.className) },
      h(VbIcon, { icon: statusIcon(kind), size: 14, className: 'vb-status__icon' }),
      L('vb-status__word', p.text || kind.charAt(0).toUpperCase() + kind.slice(1)),
      p.detail ? L('vb-status__detail', '· ' + p.detail) : null,
      p.time ? L('vb-status__time', p.time) : null);
  }

  function useValue(value, onChange) {
    var s = R.useState(value), v = s[0], set = s[1];
    R.useEffect(function () { set(value); }, [value]);
    return [v, function (n) { if (n === v) return; set(n); if (onChange) onChange(n); }];
  }

  function VbSwitch(p) {
    var s = useValue(!!p.value, p.onChange), on = s[0];
    return h('div', Object.assign({ className: cx('vb-switch', on && 'vb-switch--on', p.className), 'aria-checked': on, role: 'switch' }, clickable(function () { s[1](!on); }), { role: 'switch' }),
      h(Shape, { className: 'vb-switch__track' }, h('div', { className: 'vb-switch__knob' })),
      h('div', { className: 'vb-switch__text' }, L('vb-switch__label', p.text || ''), L('vb-switch__detail', p.detail || '', show(!!p.detail))));
  }

  function VbSlider(p) {
    var min = p.min == null ? 0 : p.min, max = p.max == null ? 1 : p.max;
    var s = useValue(p.value == null ? 0 : p.value, p.onChange), v = s[0], track = R.useRef(null);
    var t = max === min ? 0 : clamp01((v - min) / (max - min));
    function at(e) { var r = track.current.getBoundingClientRect(); s[1](min + clamp01((e.clientX - r.left - 6) / Math.max(1, r.width - 12)) * (max - min)); }
    return h('div', { className: cx('vb-slider', 'vb-u-base-field', p.className), tabIndex: 0, role: 'slider', 'aria-valuenow': Math.round(t * 100),
      onKeyDown: function (e) { var k = (max - min) / 20; if (e.key === 'ArrowRight') s[1](Math.min(max, v + k)); if (e.key === 'ArrowLeft') s[1](Math.max(min, v - k)); } },
      L('vb-u-base-field__label', p.label || ''),
      h('div', { className: 'vb-u-base-slider__input' },
        h('div', { className: 'vb-u-base-slider__drag-container', ref: track,
          onPointerDown: function (e) { e.currentTarget.setPointerCapture(e.pointerId); at(e); },
          onPointerMove: function (e) { if (e.buttons) at(e); } },
          h('div', { className: 'vb-u-base-slider__tracker' }, h('div', { className: 'vb-slider__fill', style: { width: pct(t) } })),
          h('div', { className: 'vb-u-base-slider__dragger', style: { left: 'calc((100% - 12px) * ' + t + ')' } }))),
      L('vb-slider__value', String(Math.round(t * 100))));
  }

  function VbSegmented(p) {
    var items = p.items || [], s = useValue(Math.max(0, Math.min(items.length - 1, p.selected || 0)), p.onChange);
    return h(Shape, { className: cx('vb-segmented', p.className), role: 'radiogroup' }, items.map(function (it, k) {
      return h('div', Object.assign({ key: k, className: cx('vb-segmented__option', k === s[0] && 'vb-segmented__option--on'), 'aria-checked': k === s[0] }, clickable(function () { s[1](k); }), { role: 'radio' }),
        L('vb-segmented__label', it.label), it.sub ? L('vb-segmented__sub', it.sub) : null);
    }));
  }

  /* ── Meters ───────────────────────────────────────────────────────────── */
  function VbMeter(p) {
    var v = clamp01(p.value), extra = Math.max(0, Math.min(Number(p.extra || 0), 1 - v));
    var from = p.previewFrom == null ? -1 : p.previewFrom, to = p.previewTo == null ? -1 : p.previewTo, on = from >= 0 && to > from;
    return h('div', { className: cx('vb-meter', p.className), style: p.style },
      p.before || null,
      h('div', { className: 'vb-meter__fill', style: { width: pct(v) } }),
      h('div', { className: 'vb-meter__extra', style: extra > 0 ? { left: pct(v), width: pct(extra) } : { display: 'none' } }),
      h('div', { className: 'vb-meter__preview', style: on ? { left: pct(from), width: pct(to - from) } : { display: 'none' } }),
      p.after || null,
      (p.marks || []).map(function (m, i) { return h('div', { key: i, className: 'vb-meter__mark', style: { left: pct(m) } }, L('vb-meter__mark-label', String(i + 1))); }));
  }

  // `ghost` is health before the last hit (same units as hp); `size` hud | world | large; `caption` "HP 820 / 1000".
  function HealthBar(p) {
    var max = Math.max(1, p.max || 1), stab = p.stability == null ? -1 : p.stability, hp = p.hp || 0, ghost = p.ghost == null ? -1 : p.ghost, cap = p.caption || '';
    return h('div', { className: cx('vb-health', p.exposed && 'vb-health--exposed', p.enemy && 'vb-health--enemy', p.size && st('vb-health', p.size), p.className), style: p.style },
      L('vb-health__label', cap, show(cap.length > 0)),
      h(VbMeter, { className: 'vb-health__hp', value: hp / max, extra: (p.barrier || 0) / max,
        before: h('div', { className: 'vb-health__ghost', style: ghost > hp ? { width: pct(ghost / max) } : { display: 'none' } }) }),
      h(VbMeter, { className: 'vb-health__stability', value: p.exposed ? 1 : stab, style: show(stab >= 0) }));
  }

  // `reserve`: the autopilot's soft reserve line; `queue`: up to four QueueChips (manual ones raise onCancel); `legend`: a key line.
  function PowerStrip(p) {
    var max = p.max || 100, m = Math.max(1, max), from = p.previewFrom == null ? -1 : p.previewFrom, res = p.reserve || 0, q = (p.queue || []).slice(0, 4), legend = p.legend || '';
    return h('div', { className: cx('vb-power', p.className), style: p.style },
      h('div', { className: 'vb-power__head' }, L('vb-power__label', 'POWER'), L('vb-power__value', String(p.value || 0)), L('vb-power__max', '/' + max),
        L('vb-power__regen', p.regen || ''), L('vb-power__hint', p.hint || '', show(!!p.hint))),
      h(VbMeter, { className: 'vb-power__meter', value: (p.value || 0) / m, previewFrom: from < 0 ? -1 : from / m, previewTo: (p.previewTo == null ? -1 : p.previewTo) / m, marks: p.marks,
        after: h('div', { className: 'vb-power__reserve', title: 'Autoplay keeps ' + res + ' Power in reserve', style: res > 0 ? { left: pct(res / m) } : { display: 'none' } }) }),
      h('div', { className: 'vb-power__queue', style: show(q.length > 0) }, q.map(function (e, i) { return h(QueueChip, { key: i, view: e, position: i + 1, onCancel: p.onCancel }); })),
      L('vb-power__legend', legend, show(legend.length > 0)));
  }

  /* QueueChip.cs: place, skill, operative, cost and time to fire; a full-height × on manual entries. */
  function QueueChip(p) {
    var v = Object.assign({ who: '', skill: '', eta: '', cost: 0, manual: false, next: false }, p.view), pos = p.position || 1, preview = !!p.preview, cancel = v.manual && !preview;
    return h(Shape, { className: cx('vb-queue-chip', (pos === 1 || v.next) && 'vb-queue-chip--next', preview && 'vb-queue-chip--preview', v.manual && 'vb-queue-chip--manual', p.className), style: p.style },
      L('vb-queue-chip__index', v.next ? '›' : String(pos)),
      h('div', { className: 'vb-queue-chip__text' }, L('vb-queue-chip__skill', upper(v.skill)), L('vb-queue-chip__who', v.who)),
      L('vb-queue-chip__cost', String(v.cost)), L('vb-queue-chip__eta', v.eta, show(!!v.eta)),
      h('div', Object.assign({ className: 'vb-queue-chip__cancel', title: 'Cancel queued cast', 'aria-label': 'Cancel queued cast', style: show(cancel) },
        clickable(function () { if (cancel && p.onCancel) p.onCancel(v); })), h(VbIcon, { icon: 'close', size: 14 })));
  }

  function FocusMeter(p) {
    var f = p.focus == null ? 100 : p.focus, regen = p.regenPerSecond == null ? 12.5 : p.regenPerSecond, drain = p.drainPerSecond == null ? 25 : p.drainPerSecond, scale = p.slowScale == null ? 0.2 : p.slowScale;
    var rate = p.slow ? '×' + scale.toFixed(1) + ' · −' + Math.round(drain) + '/s' : f < 100 ? '+' + trim1(regen) + '/s' : 'READY';
    return h('div', { className: cx('vb-focus', p.slow && 'vb-focus--slow', p.className), style: p.style },
      L('vb-focus__label', 'FOCUS'), h(VbMeter, { className: 'vb-focus__meter', value: f / 100 }), L('vb-focus__value', String(Math.round(f))), L('vb-focus__rate', rate));
  }

  /* ── SkillButton ──────────────────────────────────────────────────────── */
  var SKILL = { label: 'Skill', icon: 'projectile', slot: 'A', kind: 'Active', cost: 0, state: 'ready', charged: 1, seconds: 0, need: 0, queue: 0, progress: 0, reason: '' };
  var SKILL_STATES = ['ready', 'aiming', 'cooldown', 'short', 'silenced', 'fallen', 'casting', 'disabled', 'queued', 'buffered', 'windup', 'invalid', 'rejected', 'exposed'];
  // Entering Rejected flashes crimson once for 400 ms (SkillButton.Bind); `flash` holds it for previews.
  function useFlash(on) {
    var f = R.useState(false), was = R.useRef(false);
    R.useEffect(function () {
      var t = null; if (on && !was.current) { f[1](true); t = setTimeout(function () { f[1](false); }, 400); }
      was.current = on; return function () { if (t) clearTimeout(t); };
    }, [on]);
    return f[0];
  }
  function SkillButton(p) {
    var v = Object.assign({}, SKILL, p.view), s = kebab(v.state), top = '', value, flash = useFlash(s === 'rejected') || !!p.flash;
    switch (s) {
      case 'casting': value = 'CAST'; break;
      case 'disabled': value = 'LOCK'; break;
      case 'cooldown': value = seconds(v.seconds); break;
      case 'aiming': value = 'AIM'; break;
      case 'short': top = 'NEED'; value = String(v.need); break;
      case 'silenced': top = 'SIL'; value = seconds(v.seconds); break;
      case 'fallen': value = 'OUT'; break;
      case 'queued': top = 'QUEUE'; value = '#' + v.queue; break;
      case 'buffered': top = 'NEXT'; value = String(v.cost); break;
      case 'windup': value = seconds(v.seconds); break;
      case 'invalid': value = v.reason ? upper(v.reason) : 'AIM'; break;
      case 'exposed': top = 'EXPOSED'; value = String(v.cost); break;
      default: value = String(v.cost);
    }
    var tip = v.label + ' · ' + v.cost + ' Power' + (s === 'cooldown' ? ' · ' + Math.round(v.charged * 100) + '% ready' : s === 'invalid' && v.reason ? ' · ' + v.reason : s === 'rejected' ? ' · refused' : '');
    return h(Shape, Object.assign({ className: cx('vb-skill', st('vb-skill', s), flash && 'vb-skill--flash', p.dragging && 'vb-dragging', p.className), title: tip, tabIndex: 0, style: p.style }, p.onPointerDown ? { onPointerDown: p.onPointerDown } : null),
      h('div', { className: 'vb-skill__charge', style: s === 'cooldown' ? { height: pct(v.charged) } : { display: 'none' } }),
      h('div', { className: 'vb-skill__sweep', style: s === 'windup' ? { width: pct(v.progress) } : { display: 'none' } }),
      L('vb-skill__slot', v.slot),
      h(VbIcon, { icon: s === 'fallen' ? 'lock' : s === 'invalid' ? 'no-target' : v.icon, size: 20, className: 'vb-skill__icon' }),
      h('div', { className: 'vb-skill__slash', style: show(s === 'silenced' || s === 'invalid') }),
      h('div', { className: 'vb-skill__foot' }, L('vb-skill__top', top, show(top.length > 0)), L('vb-skill__value', value)));
  }
  SkillButton.states = SKILL_STATES;

  /* ── Battle HUD ───────────────────────────────────────────────────────── */
  // With onCancel, manual rows are buttons that cancel their cast; autoplay estimates are not.
  function QueueList(p) {
    var q = p.queue || [];
    return h(Shape, { className: cx('vb-queue', p.className) },
      h('div', { className: 'vb-queue__head' }, L('vb-queue__title', (p.title || 'AUTOPLAY QUEUE') + ' · ' + q.length), L('vb-queue__hint', p.hint == null ? 'Fires as Power reaches each cost' : p.hint)),
      q.map(function (e, i) {
        var manual = !!e.manual && !!p.onCancel;
        return h(Shape, Object.assign({ key: i, className: cx('vb-queue__row', i === 0 && 'vb-queue__row--next', manual && 'vb-queue__row--manual') },
          manual ? Object.assign(clickable(function () { p.onCancel(e); }), { title: 'Cancel queued cast' }) : null),
          L('vb-queue__index', e.next ? '›' : String(i + 1)),
          h('div', { className: 'vb-queue__text' }, L('vb-queue__skill', upper(e.skill)), L('vb-queue__who', e.who)),
          L('vb-queue__cost', String(e.cost || 0)), L('vb-queue__eta', e.eta || ''),
          manual ? h(VbIcon, { icon: 'cancel', size: 14, className: 'vb-queue__cancel' }) : null);
      }));
  }

  var HUD = { speed: 1, wave: 1, score: 0, boss: false, auto: true, slow: false, power: 0, maxPower: 100, regen: '+12/s', hint: null, focus: 100, previewFrom: -1, previewTo: -1, queue: [], queueTitle: 'AUTOPLAY QUEUE', queueHint: 'Fires as Power reaches each cost' };
  function HudTop(p) {
    var v = Object.assign({}, HUD, p.view), q = v.queue || [];
    var pop = usePopover(function () { return q.length ? h(QueueList, { queue: q, title: v.queueTitle, hint: v.queueHint, onCancel: p.onQueueCancel }) : null; }, 'below', false, p.queuePinned);
    return h('div', { className: cx('vb-hud', v.boss && 'vb-hud--boss', p.className), style: p.style },
      h('div', { className: 'vb-hud__row' },
        h(IconButtonComposite, { icon: 'pause', tooltip: 'Pause', quiet: true, className: 'vb-hud__pause', onClick: p.onPauseClick }),
        h(IconButtonComposite, { icon: 'automation', tooltip: v.auto ? 'Autoplay on · tap to disable' : 'Autoplay off · tap to enable', quiet: true, active: !!v.auto, className: cx('vb-hud__auto', v.auto && 'vb-hud__auto--on'), onClick: p.onAutoClick }),
        h('div', { className: 'vb-hud__stats' },
          h('div', { className: 'vb-hud__stat' }, L('vb-hud__key', v.boss ? 'BOSS' : 'WAVE'), h(GlitchText, { key: v.wave, text: pad2(v.wave), className: 'vb-hud__wave', effect: 'slam' })),
          h('div', { className: 'vb-hud__stat' }, L('vb-hud__key', 'SCORE'), L('vb-hud__score', compact(v.score)))),
        h(IconButtonComposite, { text: v.speed + '×', tooltip: 'Toggle simulation speed', className: 'vb-hud__speed', textClass: 'vb-hud__auto-state', onClick: p.onSpeedClick })),
      h(Shape, Object.assign({ ref: pop.ref, className: cx('vb-hud__resources', pop.open && 'vb-popover-open'), tabIndex: 0 }, pop.handlers),
        h('div', { className: 'vb-hud__resource-row' }, h(PowerStrip, { value: v.power, max: v.maxPower, regen: v.regen, hint: v.hint, previewFrom: v.previewFrom, previewTo: v.previewTo, marks: q.length ? [q[0].cost / Math.max(1, v.maxPower)] : null }), h(CreditsChip, { credits: v.credits || 0 })),
        h(FocusMeter, { focus: v.focus, slow: v.slow, style: show(p.showFocus !== false) })), pop.layer);
  }

  function BossBar(p) {
    var b = Object.assign({ name: '', health: 1, stability: 1, detail: '', shielded: false, interruptible: false, shieldDetail: '' }, p.view);
    return h(Shape, { className: cx('vb-boss', p.className) },
      h('div', { className: 'vb-boss__head' }, h(VbIcon, { icon: 'marker', size: 14, className: 'vb-boss__icon' }), L('vb-boss__name', upper(b.name)), L('vb-boss__detail', b.detail)),
      h(HealthBar, { className: 'vb-boss__health', hp: b.health, max: 1, barrier: 0, stability: b.stability, exposed: b.stability <= 0, enemy: true }),
      h('div', { className: 'vb-boss__chips', style: show(b.shielded || b.interruptible) },
        b.shielded ? h(StatusChip, { kind: 'barrier', text: 'Shielded', detail: b.shieldDetail }) : null,
        b.interruptible ? h(StatusChip, { kind: 'interrupted', text: 'Interruptible' }) : null));
  }

  /* ── Screens ──────────────────────────────────────────────────────────── */
  function ScreenHeader(p) {
    return h('div', { className: cx('vb-header', p.className) },
      h(IconButtonComposite, { icon: 'back', tooltip: p.backLabel || 'Back', className: 'vb-header__back', onClick: p.onBack }),
      L('vb-header__title', upper(p.title)),
      L(cx('vb-header__meta', p.accent && 'vb-header__meta--accent'), p.meta || ''));
  }

  function ScoreRow(p) {
    var v = Object.assign({ rank: 0, score: 0, wave: 0, bosses: 0, squad: [], current: false }, p.view);
    return h(ListRow, { className: cx('vb-score', v.current && 'vb-score--current', p.className), style: p.style, selected: v.current, lead: pad2(v.rank), leadClass: v.rank <= 3 && 'vb-score__rank--top',
      title: num(v.score), value: v.current ? 'THIS RUN' : '', caption: 'Wave ' + v.wave + ' · ' + v.bosses + ' ' + (v.bosses === 1 ? 'boss' : 'bosses') + (v.squad.length ? ' · ' + v.squad.join(' ') : '') });
  }


  /* ── Operatives ───────────────────────────────────────────────────────── */
  var OPERATIVE = { id: '', name: '', code: '', role: '', roleIcon: 'striker', passive: '', passiveText: '', passiveFullText: '', kit: '#ffffff', portrait: null, hp: 0, maxHp: 1, barrier: 0, armor: 0, stability: 100, maxStability: 100, statuses: [], mode: 'auto', manualSeconds: 0, skills: [{}, { slot: 'S', kind: 'Signature' }] };
  function operative(v) { return Object.assign({}, OPERATIVE, v); }
  function fallenOf(v) { return kebab(v.mode) === 'fallen' || v.hp <= 0; }
  function img(url) { return url ? { backgroundImage: 'url("' + url + '")' } : null; }
  function Strike(on, text) { return on ? h('s', null, text) : text; }

  function OperativeCard(p) {
    var v = operative(p.view), fallen = fallenOf(v), mode = fallen ? 'fallen' : kebab(v.mode);
    var chip = mode === 'manual' ? (v.manualSeconds > 0 ? 'MANUAL ' + seconds(v.manualSeconds) : 'MANUAL') : mode === 'aiming' ? 'AIMING' : mode === 'fallen' ? 'FALLEN' : 'AUTO';
    var statuses = (v.statuses || []).map(kebab).filter(function (s, i, a) { return a.indexOf(s) === i; });
    var hp = fallen ? 0 : v.hp;
    return h(Shape, { className: cx('vb-operative', st('vb-operative', mode), p.className), style: p.style },
      h('div', { className: 'vb-operative__band' }, h('div', { className: 'vb-operative__portrait', style: img(v.portrait) }), h('div', { className: 'vb-operative__shade' }),
        L('vb-operative__code', v.code), L('vb-operative__chip', chip)),
      h('div', { className: 'vb-operative__kit', style: { backgroundColor: v.kit } }),
      h('div', { className: 'vb-operative__body' },
        L('vb-operative__name', Strike(fallen, upper(v.name))),
        h(HealthBar, { hp: hp, max: v.maxHp, barrier: v.barrier, stability: fallen ? 0 : v.stability / Math.max(1, v.maxStability), exposed: statuses.indexOf('exposed') >= 0 }),
        h('div', { className: 'vb-operative__numbers' },
          h('div', { className: 'vb-l vb-operative__hp', style: { fontSize: (String(v.hp) + v.maxHp).length > 7 ? 10 : 11 } }, String(hp), h('span', { style: { color: '#b6aeb9' } }, '/' + v.maxHp)),
          L('vb-operative__barrier', v.barrier > 0 ? '+' + v.barrier : '', show(v.barrier > 0))),
        h('div', { className: 'vb-operative__statuses' },
          fallen ? L('vb-operative__clear', 'OUT') : statuses.length === 0 ? L('vb-operative__clear', 'CLEAR') :
            statuses.map(function (s) { return h(VbIcon, { key: s, icon: statusIcon(s), size: 14, title: s.charAt(0).toUpperCase() + s.slice(1), className: cx('vb-operative__status', 'vb-operative__status--' + s) }); }))),
      h('div', { className: 'vb-operative__skills', style: show(!fallen) }, [0, 1].map(function (i) {
        return h(SkillButton, { key: i, view: v.skills[i] || OPERATIVE.skills[i], dragging: p.dragging === i, onPointerDown: p.onSkillPointerDown ? function (e) { p.onSkillPointerDown(i, e); } : null });
      })),
      h('div', { className: 'vb-operative__fallen', style: show(fallen) }, L('vb-operative__fallen-title', 'FALLEN'), L('vb-operative__fallen-hint', 'Revive via perk')));
  }

  /** TileComponent (Tile.cs): picture or glyph, name, caption with its glyph, order; `state` combines in | focused, or is locked | empty. */
  function TileComponent(p) {
    var b = p.block || 'vb-tile', state = [].concat(p.state || []).map(kebab), has = function (k) { return state.indexOf(k) >= 0; };
    var locked = has('locked'), empty = has('empty'), order = p.order || 0, glyph = p.glyph && !p.picture;
    return h(Shape, Object.assign({ className: cx(b, has('in') && !locked && b + '--in', has('focused') && b + '--focused', locked && b + '--locked', empty && b + '--empty', p.tier && st(b, p.tier), p.className), style: p.style, title: p.tooltip }, clickable(p.onPick)),
      h('div', { className: b + '__band', style: show(!empty) }, h('div', { className: b + '__portrait', style: img(p.picture) }), h('div', { className: b + '__shade' }),
        glyph ? h(VbIcon, { icon: p.glyph, size: 32, className: b + '__glyph' }) : null, L(b + '__order', String(order), show(order > 0 && !locked))),
      h('div', { className: b + '__kit', style: Object.assign({ backgroundColor: p.kit }, show(!empty)) }),
      h('div', { className: b + '__body', style: show(!empty) }, L(b + '__name', upper(p.title)),
        h('div', { className: b + '__role' }, h(VbIcon, { icon: p.captionIcon || '', size: 12 }), L(b + '__role-text', upper(p.caption)))),
      empty ? h('div', { className: b + '__empty' }, h(VbIcon, { icon: 'add', size: 20 }), L(b + '__empty-label', upper(p.emptyLabel))) : null);
  }
  // `tier` none | blue | violet | gold frames the tile; `locked` ("Pull in rifts") says how to get a locked operative.
  function OperativeTile(p) {
    var v = operative(p.view), slot = p.slot || 0, locked = p.locked || '', isLocked = locked.length > 0;
    return h(TileComponent, { className: p.className, style: p.style, onPick: p.onPick, tier: p.tier || 'none', picture: v.portrait, kit: v.kit, title: v.name, caption: isLocked ? locked : v.role, captionIcon: isLocked ? 'lock' : v.roleIcon, order: slot,
      state: [slot > 0 && !isLocked && 'in', p.focused && 'focused', isLocked && 'locked'].filter(Boolean),
      tooltip: v.name + ', ' + v.role + (isLocked ? ', locked: ' + locked : slot > 0 ? ', squad slot ' + slot : ', not in squad') });
  }

  function SquadSlot(p) {
    var has = !!p.view, v = operative(p.view), slot = p.slot || 0;
    return h(TileComponent, { block: 'vb-slot', className: p.className, style: p.style, onPick: p.onPick, picture: has ? v.portrait : null, kit: v.kit, title: v.name, caption: v.code, order: has ? slot : 0, state: has ? 'in' : 'empty', emptyLabel: 'SLOT ' + slot });
  }

  function OperativeDetails(p) {
    var v = operative(p.view), slot = p.slot || 0, inSquad = slot > 0;
    var maxHp = p.maxHp || 1580, maxArmor = p.maxArmor || 26, maxStab = p.maxStability || 140;
    return h(Shape, { className: cx('vb-details', p.className), style: p.style },
      h('div', { className: 'vb-details__head' },
        h('div', { className: 'vb-details__kit', style: { backgroundColor: v.kit } }), L('vb-details__name', upper(v.name)),
        h('div', { className: 'vb-details__role' }, h(VbIcon, { icon: v.roleIcon, size: 14 }), L('vb-details__role-text', upper(v.role))),
        L(cx('vb-details__slot', inSquad && 'vb-details__slot--in'), inSquad ? 'SLOT ' + slot : 'NOT IN SQUAD'),
        h(IconButtonComposite, { icon: 'close', tooltip: 'Close details', size: 12, className: 'vb-details__close', onClick: p.onClose })),
      h('div', { className: 'vb-details__stats' },
        h(StatReadout, { label: 'HEALTH', value: num(v.maxHp), bar: v.maxHp / maxHp }),
        h(StatReadout, { label: 'ARMOR', value: String(v.armor), bar: v.armor / maxArmor, className: 'vb-stat--neutral' }),
        h(StatReadout, { label: 'STABILITY', value: String(v.maxStability), bar: v.maxStability / maxStab, className: 'vb-stat--neutral' })),
      h('div', { className: 'vb-details__passive' }, L('vb-details__passive-name', 'PASSIVE · ' + upper(v.passive)), h('div', { className: 'vb-l vb-details__passive-text', title: v.passiveFullText }, v.passiveText)),
      h('div', { className: 'vb-details__skills' }, v.skills.map(function (s0, i) {
        var s = Object.assign({}, SKILL, { description: '', fullDescription: '', cooldownSeconds: 0 }, s0);
        return h('div', { key: i, className: 'vb-details__skill', title: s.fullDescription },
          h(Shape, { className: 'vb-details__skill-icon' }, h(VbIcon, { icon: s.icon, size: 14 })),
          h('div', { className: 'vb-details__skill-text' },
            h('div', { className: 'vb-details__skill-line' }, L('vb-details__skill-name', s.label), L('vb-details__skill-slot', s.slot + ' · ' + upper(s.kind))),
            L('vb-details__skill-desc', s.description)),
          h('div', { className: 'vb-details__skill-cost' }, L('vb-details__skill-power', String(s.cost)), L('vb-details__skill-cd', seconds(s.cooldownSeconds) + ' CD')));
      })),
      h(VbButton, { className: 'vb-details__toggle', title: inSquad ? 'REMOVE FROM SQUAD' : 'ADD TO SQUAD', icon: inSquad ? 'remove' : 'add', variant: inSquad ? 'danger' : 'light', blocked: !inSquad && p.squadFull ? 'Squad full · remove one first' : '', onClick: p.onToggle }));
  }

  function OperativeStats(p) {
    var c = Object.assign({ fellWave: 0, damage: 0, kills: 0, casts: 0, power: 0, share: 0, extraLabel: 'Basics', extraValue: '0' }, p.view), v = operative(c.operative);
    var cells = [['DAMAGE', num(c.damage)], ['KILLS', num(c.kills)], ['SKILLS CAST', num(c.casts)], ['POWER SPENT', num(c.power)], [upper(c.extraLabel), c.extraValue], ['WAVES SURVIVED', c.fellWave > 0 ? String(c.fellWave - 1) : 'ALL']];
    return h(Shape, { className: cx('vb-contribution', p.className) },
      h('div', { className: 'vb-contribution__head' },
        h(Shape, { className: 'vb-contribution__portrait' }, h('div', { className: 'vb-crop', style: img(v.portrait) })),
        h('div', { className: 'vb-contribution__head-text' }, L('vb-contribution__name', upper(v.name)),
          L(cx('vb-contribution__fell', c.fellWave <= 0 && 'vb-contribution__fell--alive'), c.fellWave > 0 ? 'FELL WAVE ' + c.fellWave : 'STANDING'))),
      h('div', { className: 'vb-contribution__share' },
        h('div', { className: 'vb-contribution__share-line' }, L('vb-contribution__share-label', 'Share of squad damage'), L('vb-contribution__share-value', Math.round(c.share * 100) + '%')),
        h('div', { className: 'vb-contribution__share-track' }, h('div', { className: 'vb-contribution__share-fill', style: { width: pct(c.share), backgroundColor: v.kit } }))),
      h('div', { className: 'vb-contribution__grid' }, cells.map(function (kv, i) { return h('div', { key: i, className: 'vb-contribution__cell' }, L('vb-contribution__key', kv[0]), L('vb-contribution__value', kv[1])); })));
  }

  function SquadResult(p) {
    var c = Object.assign({ fellWave: 0 }, p.view), v = operative(c.operative), fell = c.fellWave > 0;
    var pop = usePopover(function () { return h(OperativeStats, { view: c }); }, 'above', false, p.pinned);
    return h(Shape, Object.assign({ ref: pop.ref, className: cx('vb-result', fell && 'vb-result--fallen', pop.open && 'vb-popover-open', p.className), tabIndex: 0 }, pop.handlers),
      L('vb-result__name', Strike(fell, upper(v.name))), L('vb-result__fell', fell ? 'FELL W' + c.fellWave : 'STANDING'),
      h('div', { className: 'vb-result__kit', style: { backgroundColor: v.kit } }), pop.layer);
  }

  /* ── Perks ────────────────────────────────────────────────────────────── */
  var PERK = { id: '', name: '', category: '', tag: 'General', icon: 'add', effect: '', now: '', next: '', tier: 'general', art: null, stack: 0, max: 0, waves: [] };
  function perk(p) { return Object.assign({}, PERK, p); }
  function stacks(p, draft) {
    return p.max > 0 ? (draft ? 'STACK ' + (p.stack + 1) + ' / ' + p.max : '×' + p.stack + ' OF ' + p.max) : kebab(p.tier) === 'rare' ? 'RARE DRAW' : p.stack > 1 ? '×' + p.stack : 'REPEATABLE';
  }
  function tier(p) { return st('vb-perk', p.tier); }
  function Crop(url) { return h('div', { className: 'vb-crop', style: Object.assign({}, img(url), show(!!url)) }); }

  /** ProgressBar (Progress.cs): continuous (`value` 0–1) or `segments` levels filled to `value`; `next` outlines the level a pick adds. */
  function ProgressBar(p) {
    var n = p.segments || 0, v = p.value || 0, filled = Math.round(v);
    return h('div', { className: cx('vb-progress', st('vb-progress', p.tone || 'ally'), n > 0 && 'vb-progress--segmented', p.className), style: p.style, role: 'progressbar', 'aria-valuenow': n ? filled : Math.round(clamp01(v) * 100), 'aria-valuemax': n || 100 },
      n > 0 ? Array.apply(null, Array(n)).map(function (_, i) { return h('div', { key: i, className: cx('vb-progress__cell', i < filled && 'vb-progress__cell--on', p.next && i === filled && 'vb-progress__cell--next') }); })
        : h('div', { className: 'vb-progress__fill', style: { width: pct(v) } }));
  }
  /** ListRow (Progress.cs): glyph, caps title, caption, optional `bar` (ProgressBar props), trailing `value` and `action`. */
  function ListRow(p) {
    return h(Shape, Object.assign({ className: cx('vb-list-row', p.selected && 'vb-list-row--selected', p.className), style: p.style }, p.onPick ? clickable(p.onPick) : null),
      p.lead ? L(cx('vb-list-row__lead', p.leadClass), p.lead) : null, p.icon ? h(VbIcon, { icon: p.icon, size: 20, className: 'vb-list-row__icon' }) : null,
      h('div', { className: 'vb-list-row__text' }, p.children, L('vb-list-row__title', upper(p.title)), p.caption ? L('vb-t-caption vb-list-row__caption', p.caption) : null,
        p.bar ? h(ProgressBar, Object.assign({ className: 'vb-list-row__bar' }, p.bar)) : null),
      p.value ? L(cx('vb-list-row__value', p.valueClass), p.value) : null, p.action ? h('div', { className: 'vb-list-row__trail' }, p.action) : null);
  }
  /** PerkCard, minimal: tier word, glyph, name, effect, its levels as a segmented ProgressBar and the value after the pick. */
  function PerkCard(p) {
    var v = perk(p.view);
    return h(Shape, Object.assign({ className: cx('vb-perk-card', tier(v), p.selected && 'vb-perk-card--selected', p.className), style: p.style, title: v.name + ' · ' + stacks(v, true) }, clickable(p.onPick)),
      L('vb-t-badge vb-perk-card__tag', upper(v.tag)), h(VbIcon, { icon: v.icon, size: 28, className: 'vb-perk-card__glyph' }),
      L('vb-perk-card__name', upper(v.name)), L('vb-t-caption vb-perk-card__effect', v.effect), h('div', { className: 'vb-grow' }),
      v.max > 0 ? h(ProgressBar, { segments: v.max, value: v.stack, next: v.stack < v.max, className: 'vb-perk-card__levels' }) : null,
      L('vb-perk-card__next', v.next));
  }


  function PerkDetail(p) {
    var v = perk(p.view), draft = !!p.draft, art = !!v.art;
    var rows = draft ? [['Now', v.now, ''], ['After pick', v.next, '--good'], ['Stacks', v.max > 0 ? v.stack + ' → ' + (v.stack + 1) + ' / ' + v.max : stacks(v, true), '']]
      : [['Active', v.now, '--good'], ['Stacks', v.max > 0 ? v.stack + ' / ' + v.max : stacks(v, false), ''], ['Picked', v.waves.length ? v.waves.map(function (w) { return 'wave ' + w; }).join(', ') : '—', '']];
    return h(Shape, { className: cx('vb-perk-detail', tier(v), p.className), style: p.style },
      h('div', { className: 'vb-perk-detail__head' },
        h(Shape, { className: cx('vb-perk-detail__thumb', 'vb-perk-art', art && 'vb-perk-art--portrait') }, Crop(v.art), h(VbIcon, { icon: v.icon, size: 24 })),
        h('div', { className: 'vb-perk-detail__head-text' }, L('vb-perk-detail__name', upper(v.name)), L('vb-perk-detail__tag', upper(v.category + ' · ' + v.tag)))),
      L('vb-perk-detail__effect', v.effect),
      h('div', { className: 'vb-perk-detail__rows' }, rows.map(function (r, i) {
        return h('div', { key: i, className: cx('vb-perk-detail__row', i === rows.length - 1 && 'vb-perk-detail__row--last') },
          L('vb-perk-detail__key', r[0]), L(cx('vb-perk-detail__value', r[2] && 'vb-perk-detail__value' + r[2]), r[1]));
      })));
  }

  function PerkChip(p) {
    var v = perk(p.view);
    var pop = usePopover(function () { return h(PerkDetail, { view: v, draft: false, className: 'vb-perk-chip__popover' }); }, 'below', !!p.alignEnd, p.pinned);
    return h(Shape, Object.assign({ ref: pop.ref, className: cx('vb-perk-chip', tier(v), pop.open && 'vb-popover-open', p.className), tabIndex: 0, title: v.name + ' · ' + v.now }, pop.handlers),
      h(VbIcon, { icon: v.icon, size: 14 }), L('vb-perk-chip__name', upper(v.name)), L('vb-perk-chip__count', '×' + v.stack, show(v.stack > 1)), pop.layer);
  }

  function PerkRow(p) {
    var v = perk(p.view);
    return h(ListRow, { className: cx('vb-perk-row', tier(v), p.className), style: p.style, selected: p.selected, onPick: p.onPick || function () { }, icon: v.icon, title: v.name, caption: v.tag + ' · ' + v.effect, value: v.now,
      bar: v.max > 0 ? { segments: v.max, value: v.stack } : null });
  }


  /* ── Pages (Dialogs.cs) ───────────────────────────────────────────────── */
  /** Dialog.Sheet: a solid panel with a title and a line of body text; children are the actions. */
  function Dialog(p) {
    return h(Shape, { className: cx('vb-panel', 'vb-dialog', 'vb-panel--solid', p.className), style: p.style },
      p.title ? L('vb-t-title vb-dialog__title', p.title) : null,
      p.body ? L(cx(p.bodyClass || 'vb-t-caption', 'vb-dialog__body'), p.body) : null, p.children);
  }
  function Backdrop(p) {
    return h('div', { className: cx('vb-backdrop', p.className), style: Object.assign({}, img(p.image), p.style) }, h(Shape, { className: 'vb-backdrop__shade' }), p.children);
  }

  /* ── Holographic menu surfaces (Holo.cs) ──────────────────────────────── */
  function ScanLines(p) { return h('div', { className: cx('vb-scanlines', p && p.className), 'aria-hidden': true }); }
  function calm(el, flashes) { return !!(el && el.closest && el.closest(flashes ? '.vb-reduced-motion, .vb-reduced-flashes' : '.vb-reduced-motion')); }
  /** HoloFrame.FlickerIn: opacity .7 → .25 → .9 → 1 in 60 ms steps; a plain fade under reduced motion or flashes. */
  function HoloFrame(p) {
    var ref = R.useRef(null), t = upper(p.title || '');
    useIsoLayout(function () {
      var el = ref.current; if (!el || p.still) return undefined;
      var steps = calm(el, true) ? [0.25, 0.5, 0.75, 1] : [0.7, 0.25, 0.9, 1]; el.style.opacity = 0;
      var timers = steps.map(function (o, i) { return setTimeout(function () { el.style.opacity = o; }, 60 * (i + 1)); });
      return function () { timers.forEach(clearTimeout); el.style.opacity = ''; };
    }, []);
    return h(Shape, { ref: ref, className: cx('vb-holo', p.className), style: p.style },
      h(ScanLines),
      h('div', { className: 'vb-holo__head', style: show(t.length > 0) },
        L('vb-holo__title vb-holo__title--cyan', t), L('vb-holo__title vb-holo__title--pink', t), L('vb-holo__title', t)),
      h('div', { className: 'vb-holo__body' }, p.children));
  }
  /** The 2D stand-in for a lineup plinth: occupied, selected (brackets) or open (dashed OPEN SLOT). */
  function HoloPlatform(p) {
    var state = kebab(p.state || 'occupied'), open = state === 'open', label = open ? '+ OPEN SLOT' : upper(p.name), sub = open ? 'weight shared' : (p.detail || '');
    return h('div', Object.assign({ className: cx('vb-platform', st('vb-platform', state), st('vb-platform', p.team || 'ally'), p.className), style: p.style, title: open ? 'Open slot' : p.name }, clickable(p.onPick)),
      h(VbIcon, { icon: 'diamond', size: 18, className: 'vb-platform__marker', style: show(p.showMarker !== false && !open) }),
      h('div', { className: 'vb-platform__tag' }, L('vb-platform__label', label), L('vb-platform__sub', sub, show(sub.length > 0))),
      h(Shape, { className: 'vb-platform__plate' }, h(ScanLines)));
  }
  function tierWord(tier, item) {
    tier = kebab(tier);
    return item ? (tier === 'gold' ? 'UNIQUE ITEM' : tier === 'violet' ? 'RARE ITEM' : 'MAGIC ITEM') : (tier === 'gold' ? 'LEGENDARY' : tier === 'violet' ? 'EPIC' : 'RARE');
  }
  /** RevealCard.Flip: an edge-on squash (violet waits 90 ms, gold 180 ms), or at once under reduced motion. */
  function RevealCard(p) {
    var tier = kebab(p.tier || 'blue'), f = R.useState(!!p.flipped), up = f[0], ref = R.useRef(null);
    R.useEffect(function () { f[1](!!p.flipped); }, [p.flipped]);
    function flip() {
      if (up) return; if (p.onFlip) p.onFlip(); var el = ref.current;
      if (!el || calm(el, false)) { f[1](true); return; }
      var wait = tier === 'gold' ? 180 : tier === 'violet' ? 90 : 0;
      setTimeout(function () { el.style.scale = '0.05 1'; }, wait);
      setTimeout(function () { f[1](true); }, wait + 110);
      setTimeout(function () { el.style.scale = ''; }, wait + 130);
    }
    var badge = p.dupe ? upper(p.dupe) : p.isNew !== false ? 'NEW' : '';
    return h('div', Object.assign({ ref: ref, className: cx('vb-reveal', st('vb-reveal', tier), up && 'vb-reveal--flipped', p.className), style: p.style }, clickable(flip)),
      h(Shape, { className: 'vb-reveal__back', style: show(!up) }, h(ScanLines), h(VbMark, { width: 56 })),
      h(Shape, { className: 'vb-reveal__front', style: show(up) },
        L('vb-reveal__tier', p.tierLabel || tierWord(tier, p.item)), L('vb-reveal__name', upper(p.title)), L('vb-reveal__line', p.detail || ''),
        L(cx('vb-reveal__badge', p.dupe && 'vb-reveal__badge--dupe'), badge, show(badge.length > 0))));
  }
  RevealCard.tierWord = tierWord;
  /** One IF … THEN automation rule (RuleView); the toggle raises onToggle(enabled). */
  function RuleRow(p) {
    var init = Object.assign({ index: 1, conditions: [], any: false, action: '', actionRest: '', cost: 0, enabled: true, warning: false, draft: false, note: '' }, p.view);
    var e = R.useState(init.enabled), on = e[0], set = e[1]; R.useEffect(function () { set(init.enabled); }, [init.enabled]);
    function toggle() { set(!on); if (p.onToggle) p.onToggle(!on); }
    var cond = [h('div', { key: 'k', className: 'vb-l vb-rule__keyword' }, 'IF')];
    init.conditions.forEach(function (c, i) {
      if (i) cond.push(h('div', { key: 'j' + i, className: 'vb-l vb-rule__join' }, init.any ? 'OR' : 'AND'));
      cond.push(h('div', { key: 't' + i, className: 'vb-l vb-rule__text' }, c.text)); if (c.value) cond.push(h('div', { key: 'v' + i, className: 'vb-l vb-rule__value' }, c.value)); if (c.after) cond.push(h('div', { key: 'a' + i, className: 'vb-l vb-rule__text' }, c.after));
    });
    return h(ListRow, { className: cx('vb-rule', !on && 'vb-rule--off', init.warning && 'vb-rule--warning', init.draft && 'vb-rule--draft', p.className), style: p.style, lead: String(init.index),
      title: 'THEN ' + init.action + (init.actionRest ? ' ' + init.actionRest : ''), caption: init.note, value: init.cost > 0 ? '−' + init.cost : '', valueClass: 'vb-rule__cost',
      action: h('div', Object.assign({ className: cx('vb-rule__toggle', on && 'vb-rule__toggle--on'), title: on ? 'Turn rule off' : 'Turn rule on', 'aria-pressed': on }, clickable(toggle)), h('div', { className: 'vb-rule__knob' })) },
      h('div', { className: 'vb-rule__if' }, cond));
  }


  /* ── World layer (WorldElements.cs); WorldLayer itself places them over a 3D view ── */
  function WorldHealthBar(p) {
    var hp = clamp01(p.health == null ? 1 : p.health), ghost = Math.max(hp, clamp01(p.ghost == null ? hp : p.ghost)), b = Math.min(clamp01(p.barrier), 1 - hp);
    return h('div', { className: cx('vb-world-health', st('vb-world-health', p.team || 'enemy'), st('vb-world-health', p.rank || 'normal'), p.className), style: p.style },
      h('div', { className: 'vb-world-health__ghost', style: { width: pct(ghost) } }),
      h('div', { className: 'vb-world-health__fill', style: Object.assign({ width: pct(hp) }, p.tint ? { backgroundColor: p.tint } : null) }),
      h('div', { className: 'vb-world-health__barrier', style: b > 0 ? { left: pct(hp), width: pct(b) } : { display: 'none' } }));
  }
  function combatText(kind, amount) {
    var n = num(amount);
    switch (kebab(kind)) {
      case 'taken': return '−' + n; case 'crit': return n + '!'; case 'exposed': return 'EXPOSED ' + n; case 'heal': return '+' + n;
      case 'barrier': return 'BLOCKED ' + n; case 'power': return '+' + n + ' POWER'; case 'miss': return 'MISS'; case 'execute': return 'EXECUTE ' + n; case 'break': return 'BREAK';
      default: return n;
    }
  }
  function CombatText(p) { var k = kebab(p.kind || 'damage'); return L(cx('vb-combat-text', st('vb-combat-text', k), p.className), combatText(k, p.amount), p.style); }
  CombatText.format = combatText;
  function WorldLabel(p) {
    var k = kebab(p.kind || 'name'), d = p.detail || '';
    return h('div', { className: cx('vb-world-label', st('vb-world-label', k), st('vb-world-label', p.team || 'ally'), p.className), style: p.style },
      L('vb-world-label__title', (k === 'blocked' ? '× ' : '') + (p.title || '')), L('vb-world-label__detail', d, show(d.length > 0 && p.showDetail !== false)));
  }
  function WorldMarker(p) {
    var k = kebab(p.kind || 'ally');
    return h('div', { className: cx('vb-world-marker', st('vb-world-marker', k), p.className), style: Object.assign({}, p.size > 0 ? { width: p.size, height: p.size } : null, p.style) },
      h(VbIcon, { icon: k === 'enemy' ? 'diamond' : 'circle', className: 'vb-world-marker__glyph', style: { width: '100%', height: '100%' } }));
  }
  /** `angle`: degrees clockwise from screen-right toward the fighter (WorldLayer.Pin). */
  function OffscreenIndicator(p) {
    var m = p.metres == null ? -1 : p.metres;
    return h('div', { className: cx('vb-offscreen', st('vb-offscreen', p.team || 'enemy'), p.threat && 'vb-offscreen--threat', p.className), style: p.style },
      h(VbIcon, { icon: 'chevron', size: 12, className: 'vb-offscreen__arrow', style: { rotate: (p.angle || 0) + 'deg' } }),
      L('vb-offscreen__name', p.name || ''), L('vb-offscreen__distance', m >= 0 ? Math.round(m) + 'm' : '', show(m >= 0)));
  }

  /** The breach band: chevrons over the bottom (or `top`) edge; `urgency` 0 hides it, above 2/3 it runs hot. Static here (the Unity element ripples). */
  function EdgeWarning(p) {
    var u = p.urgency == null ? 1 : p.urgency;
    return h('div', { className: cx('vb-edge-warning', p.top && 'vb-edge-warning--top', u > 0.66 && 'vb-edge-warning--hot', p.className), style: Object.assign({ display: u > 0 ? 'flex' : 'none' }, p.style) },
      h('div', { className: 'vb-edge-warning__row' }, [0, 1, 2, 3, 4].map(function (i) {
        return h(VbIcon, { key: i, icon: 'chevron', size: 14, className: 'vb-edge-warning__chevron', style: { opacity: 0.35 + 0.65 * (1 - Math.abs(i - 2) / 2) } });
      })),
      L('vb-edge-warning__caption', p.caption || ''));
  }

  function GlitchText(p) {
    return h('div', { className: cx('vb-glitch', 'vb-web-glitch--' + (p.effect || 'in'), p.className), style: p.style },
      L('vb-glitch__main', p.text),
      h('div', { className: 'vb-glitch__copy vb-glitch__red', 'aria-hidden': true }, p.text),
      h('div', { className: 'vb-glitch__copy vb-glitch__cyan', 'aria-hidden': true }, p.text), Array.apply(null, Array(8)).map(function (_, i) { return h('div', { key: i, className: 'vb-glitch__clip', 'aria-hidden': true, style: { top: (i * 12.5) + '%', height: '12.5%', width: '100%', animationDelay: (i % 3 * -16) + 'ms' } }, h('div', { className: 'vb-glitch__copy vb-glitch__slice', style: { top: (-i * 100) + '%', height: '800%', width: '100%' } }, p.text)); }));
  }
  function CreditsChip(p) { return h(Shape, { className: cx('vb-credits', p.className), title: 'Credits' }, h(VbIcon, { icon: 'currency', size: 14 }), L('vb-credits__value', num(p.credits))); }
  function RouteChoice(p) {
    var v = p.view || {}, locked = R.useRef(false); R.useEffect(function () { locked.current = false; }, [v.wave]);
    return h('div', { className: cx('vb-route', p.className), style: p.style }, L('vb-t-label vb-route__eyebrow', 'CROSSROADS · WAVE ' + pad2(v.wave)), h(GlitchText, { text: 'CHOOSE YOUR ROAD', className: 'vb-t-display vb-route__title' }), L('vb-t-caption', 'Two roads. One choice. Your build carries forward.'),
      h('div', { className: 'vb-route__roads' }, [v.left || {}, v.right || {}].map(function (road, side) { return h(Shape, Object.assign({ key: side, className: 'vb-route__sign vb-route__sign--' + (road.kind || 'combat') }, clickable(function () { if (!locked.current && p.onChoose) { locked.current = true; p.onChoose(side); } })), L('vb-t-badge vb-route__direction', side ? 'RIGHT ROAD →' : '← LEFT ROAD'), h(VbIcon, { icon: road.icon || 'battle', size: 36, className: 'vb-route__glyph' }), L('vb-route__kind', upper(road.title)), L('vb-route__hint', road.hint), h('div', { className: 'vb-grow' }), L('vb-route__confirm', 'TAKE THIS ROAD')); })));
  }
  /** MerchantScreen, minimal: one list of ListRows (rare finds, stack sales, health trades) and LEAVE MARKET. */
  function MerchantScreen(p) {
    var v = Object.assign({ credits: 0, offers: [], stacks: [], health: [] }, p.view);
    function section(t) { return h(LabelComponent, { key: 's' + t, text: t, className: 'vb-merchant__section' }); }
    function bar(k) { return k.max > 0 ? { segments: k.max, value: k.stack } : null; }
    return h('div', { className: cx('vb-merchant', p.className), style: p.style },
      h('div', { className: 'vb-merchant__head' }, L('vb-t-title', 'NIGHT MARKET'), h(CreditsChip, { credits: v.credits })),
      h(ListLayout, { className: 'vb-merchant__scroll' },
        section('Rare finds'), v.offers.map(function (o, i) { return h(ListRow, { key: 'o' + i, className: tier(o.perk), title: o.perk.name, caption: o.perk.effect, icon: o.perk.icon, bar: bar(o.perk),
          action: h(VbButton, { title: o.taken ? 'SOLD' : 'BUY ' + o.price, blocked: o.taken ? 'Sold' : v.credits < o.price ? 'Need ' + (o.price - v.credits) : o.blocked, className: 'vb-merchant__buy', onClick: function () { call(p.onBuy)(i); } }) }); }),
        section('Sell stacks'), v.stacks.length ? v.stacks.map(function (k, i) { return h(ListRow, { key: 'k' + i, title: k.perk.name, caption: k.perk.now, icon: k.perk.icon, bar: bar(k.perk), action: h(VbButton, { title: '+' + k.payout, className: 'vb-merchant__sell', onClick: function () { call(p.onSellPerk)(k.perk.id); } }) }); }) : h('div', { key: 'e', className: 'vb-l vb-t-caption vb-list__empty' }, 'Nothing to sell yet.'),
        section('Trade health · 25% max HP'), v.health.map(function (x) { return h(ListRow, { key: 'h' + x.actor, title: x.name, caption: x.hp + ' → ' + Math.max(0, x.hp - x.cost) + ' HP', icon: 'health', bar: { value: x.hp / Math.max(1, x.maxHp) },
          action: h(VbButton, { title: '+' + x.payout, variant: 'danger', blocked: x.blocked, className: 'vb-merchant__sell', onClick: function () { call(p.onSellHealth)(x.actor); } }) }); })),
      h(VbButton, { title: 'LEAVE MARKET', variant: 'primary', icon: 'chevron', className: 'vb-merchant__leave', onClick: p.onLeave }));
  }


  /* ── Text, label, badge, icon button, card (Text.cs, Badge.cs, IconButton.cs, Card.cs) ── */
  /** TextComponent: one type style (display | title | label | badge | body | caption | num) and a tone (ally | enemy | power | muted). */
  function TextComponent(p) {
    var tone = p.tone && kebab(p.tone) !== 'default' ? 'vb-t-' + kebab(p.tone) : null;
    return h('div', { className: cx('vb-l', 'vb-text', 'vb-t-' + kebab(p.typeStyle || 'body'), tone, p.className), style: p.style }, p.text != null ? p.text : p.children);
  }
  /** LabelComponent: the caps eyebrow (size label | badge); uppercases what it is given and keeps 4 under it. */
  function LabelComponent(p) {
    return h(TextComponent, { typeStyle: kebab(p.size) === 'badge' ? 'badge' : 'label', tone: p.tone, className: cx('vb-label', p.className), style: p.style, text: upper(p.text) });
  }
  /** BadgeComponent: a slim chip with a word, an optional glyph and value; tones neutral | ally | enemy | power | warn | inverted. */
  function BadgeComponent(p) {
    var value = p.value == null ? '' : String(p.value);
    return h(Shape, { className: cx('vb-badge', st('vb-badge', p.tone || 'neutral'), p.className), style: p.style, title: p.tooltip },
      p.icon ? h(VbIcon, { icon: p.icon, size: 12, className: 'vb-badge__icon' }) : null,
      p.text ? L('vb-badge__text', upper(p.text)) : null, value ? L('vb-badge__value', value) : null, p.children);
  }
  /** IconButtonComposite: a 44 × 44 chipped square with one glyph (or a short state) and a tooltip; `active` and `quiet` looks. */
  function IconButtonComposite(p) {
    return h(Shape, Object.assign({ className: cx('vb-icon-button', p.active && 'vb-icon-button--active', p.quiet && 'vb-icon-button--quiet', p.className), style: p.style,
      title: p.tooltip, 'aria-label': p.tooltip, 'aria-pressed': p.active == null ? undefined : !!p.active }, clickable(p.onClick)),
      p.icon ? h(VbIcon, { icon: p.icon, size: p.size || 20, className: 'vb-icon-button__icon' }) : null,
      p.text ? L(cx('vb-icon-button__text', p.textClass), p.text) : null);
  }
  /** VbFrame with a kind: slim (containers) or chipped (what you act on). */
  function frameClass(kind) { kind = kebab(kind || ''); return kind === 'slim' ? 'vb-frame--slim' : kind === 'chipped' ? 'vb-frame--chipped' : null; }
  /** CardComponent: slim frame, optional media band, head (title + meta), body (children), foot (actions, right-aligned). */
  function CardComponent(p) {
    var foot = [].concat(p.actions || []).filter(Boolean);
    return h(Shape, { className: cx('vb-card', p.selected && 'vb-card--selected', p.className), style: p.style },
      p.media ? h('div', { className: 'vb-card__media' }, p.media) : null,
      p.title || p.meta ? h('div', { className: 'vb-card__head' }, h(LabelComponent, { text: p.title || '', className: 'vb-card__title' }), p.meta ? L('vb-t-badge vb-card__meta', p.meta) : null) : null,
      h('div', { className: 'vb-card__body' }, p.children),
      foot.length ? h('div', { className: 'vb-card__foot' }, foot) : null);
  }

  /* ── Field, form (Forms.cs) ── */
  /** FieldComposite: a caps label, the input (children) and one help line. */
  function FieldComposite(p) {
    return h('div', { className: cx('vb-field', p.className), style: p.style },
      p.label ? h(LabelComponent, { text: p.label, size: 'badge', className: 'vb-field__label' }) : null,
      h('div', { className: 'vb-field__input' }, p.children),
      p.help ? L('vb-t-caption vb-field__help', p.help) : null);
  }
  /** Ui.Section: a titled fieldset (vb-section) for FormComposite. */
  function Fieldset(p) {
    return h(Shape, { className: cx('vb-panel', 'vb-section', p.className), style: p.style }, h(LabelComponent, { text: p.title, className: 'vb-section__title' }), p.children);
  }
  /** FormComposite: fieldsets (children) in a scroll, an optional `note` { icon, text }, a `footer` of actions and a `saved` line. */
  function FormComposite(p) {
    var footer = [].concat(p.footer || []).filter(Boolean);
    return h('div', { className: cx('vb-form', p.className), style: p.style },
      p.saved && !p.headed ? L('vb-t-badge vb-form__status', p.saved) : null,
      h('div', { className: 'vb-form__scroll vb-page__scroll', style: { overflowY: 'auto' } },
        h('div', { className: 'vb-u-scroll-view__content-container' }, p.children,
          p.note ? h(Shape, { className: 'vb-panel vb-panel--raised vb-form__note' }, h(VbIcon, { icon: p.note.icon || 'info', size: 16 }), L('vb-t-caption', p.note.text)) : null)),
      footer.length ? h('div', { className: 'vb-form__footer' }, footer) : null);
  }

  /* ── Layouts (Layouts.cs) ── */
  function StackLayout(p) { var g = p.gap <= 4 ? 4 : p.gap <= 8 || p.gap == null ? 8 : p.gap <= 12 ? 12 : 16; return h('div', { className: cx('vb-stack', 'vb-stack--' + g, p.className), style: p.style }, p.children); }
  function ListLayout(p) {
    var kids = R.Children.toArray(p.children);
    return h('div', { className: cx('vb-list-scroll vb-page__scroll', p.className), style: Object.assign({ overflowY: 'auto' }, p.style) },
      p.empty && !kids.length ? L('vb-t-caption vb-list__empty', p.empty) : null, h('div', { className: 'vb-list' }, kids));
  }
  function VbGrid(p) { var n = p.columns <= 2 ? 2 : p.columns >= 4 || p.columns == null ? 4 : 3; return h('div', { className: cx('vb-grid', 'vb-grid--' + n, p.wrap && 'vb-grid--wrap', p.className), style: p.style }, p.children); }
  function BoardLayout(p) {
    return h('div', { className: cx('vb-board', p.className), style: p.style },
      h('div', { className: 'vb-board__top' }, p.top), h('div', { className: 'vb-board__world' }, p.world), h('div', { className: 'vb-board__bottom' }, p.bottom));
  }

  /* ── Bottom drawer (Drawer.cs) ── */
  /** DrawerOverlay: a slim sheet that slides up over a backdrop to half the screen. Drag the grip or head: up snaps to full screen,
      down back to half or, below a quarter, closes it; a tap on the grip toggles half/full. The backdrop and × dismiss it too.
      `open` (default true) slides it in and out; `snap` 'half' | 'full' is where it opens; `backdrop: false` docks it in a view. */
  function DrawerOverlay(p) {
    var want = p.open !== false, dismissible = p.dismissible !== false, hasBackdrop = p.backdrop !== false, closeButton = p.closeButton !== false && dismissible;
    var sh = R.useState(false), shown = sh[0], setShown = sh[1], sn = R.useState(p.snap || 'half'), snap = sn[0], setSnapState = sn[1];
    var dh = R.useState(null), dragH = dh[0], setDragH = dh[1], live = R.useState(want), mounted = live[0], setMounted = live[1];
    var root = R.useRef(null), sheet = R.useRef(null), d = R.useRef(null);
    function setSnap(v) { setSnapState(v); if (p.onSnap) p.onSnap(v); }
    R.useEffect(function () {
      if (want) { setMounted(true); setSnapState(p.snap || 'half'); var id = requestAnimationFrame(function () { requestAnimationFrame(function () { setShown(true); }); }); return function () { cancelAnimationFrame(id); }; }
      setShown(false); var t = setTimeout(function () { setMounted(false); setDragH(null); }, 240); return function () { clearTimeout(t); };
    }, [want]);
    function dismiss() { if (!dismissible || !shown) return false; setShown(false); if (p.onDismiss) p.onDismiss(); return true; }
    function drag(isGrip) {
      return {
        onPointerDown: function (e) { if (!shown) return; d.current = { y: e.clientY, h: sheet.current.offsetHeight, moved: false, grip: isGrip }; e.currentTarget.setPointerCapture(e.pointerId); },
        onPointerMove: function (e) {
          var g = d.current; if (!g) return; var dy = e.clientY - g.y; if (Math.abs(dy) > 4) g.moved = true; if (!g.moved) return;
          var full = root.current.offsetHeight; setDragH(Math.max(dismissible ? 0 : full / 2, Math.min(full, g.h - dy)));
        },
        onPointerUp: function () {
          var g = d.current; d.current = null; if (!g) return;
          if (!g.moved) { if (g.grip) setSnap(snap === 'full' ? 'half' : 'full'); setDragH(null); return; }
          var full = root.current.offsetHeight, hgt = dragH == null ? g.h : dragH;
          if (hgt < full * 0.25 && dismiss()) return;   // keep the let-go height while it slides away
          setSnap(hgt < full * 0.75 ? 'half' : 'full'); setDragH(null);
        },
        onPointerCancel: function () { d.current = null; setDragH(null); }
      };
    }
    if (!mounted && !want) return null;
    var dragging = dragH != null && !!d.current, head = p.title || p.meta || closeButton, foot = [].concat(p.actions || []).filter(Boolean);
    return h('div', { ref: root, className: cx('vb-drawer', shown && 'vb-drawer--open', snap === 'full' && 'vb-drawer--full', dragging && 'vb-drawer--dragging', !hasBackdrop && 'vb-drawer--inline', p.className), style: Object.assign({ pointerEvents: hasBackdrop && shown ? 'auto' : 'none' }, p.style) },
      hasBackdrop ? h('div', { className: 'vb-drawer__backdrop', onPointerDown: dismiss }) : null,
      h(Shape, { ref: sheet, className: 'vb-drawer__sheet', role: 'dialog', 'aria-modal': hasBackdrop || undefined, 'aria-label': p.title || p.label, style: Object.assign({ pointerEvents: 'auto' }, dragH != null ? { height: dragH } : null) },
        h('div', Object.assign({ className: 'vb-drawer__grip', role: 'button', tabIndex: 0, 'aria-label': snap === 'full' ? 'Half screen' : 'Full screen', title: 'Drag up for full screen, down to close', style: { touchAction: 'none', cursor: 'grab' },
          onKeyDown: function (e) { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); setSnap(snap === 'full' ? 'half' : 'full'); } } }, drag(true)), h('div', { className: 'vb-drawer__handle' })),
        head ? h('div', Object.assign({ className: 'vb-drawer__head', style: { touchAction: 'none' } }, drag(false)),
          p.title ? L('vb-t-title vb-drawer__title', upper(p.title)) : h('div', { className: 'vb-drawer__title' }), p.meta ? L('vb-t-badge vb-drawer__meta', p.meta) : null,
          closeButton ? h('div', { onPointerDown: function (e) { e.stopPropagation(); } }, h(IconButtonComposite, { icon: 'close', tooltip: 'Close', className: 'vb-drawer__close', onClick: dismiss })) : null) : null,
        h('div', { className: 'vb-drawer__body vb-page__scroll', style: { overflowY: 'auto' } }, p.children),
        foot.length ? h('div', { className: 'vb-drawer__foot' }, foot) : null));
  }

  /* ── Views (Runtime/Views) ── */
  function View(block, p, kids, headed) { return h('div', { className: cx('vb-view', block, headed && 'vb-view--headed', p.className), style: p.style }, kids); }
  function call(fn) { return function () { if (fn) fn.apply(null, arguments); }; }
  /** MenuActionsSectionView: checkpoint card (when there is one), PLAY, then HIGH SCORES and SETTINGS. */
  function MenuActionsSectionView(p) {
    var c = p.checkpoint;
    return h('div', { className: cx('vb-menu-actions', p.className), style: p.style },
      c ? h(CardComponent, { title: 'CHECKPOINT', meta: 'WAVE ' + pad2(c.wave), className: 'vb-menu__checkpoint', actions: h(VbButton, { title: 'RESUME', icon: 'play', onClick: call(p.onResume) }) },
        L('vb-menu__checkpoint-line', 'Wave ' + pad2(c.wave) + ' next · ' + num(c.score)),
        L('vb-t-caption', (c.standing == null ? 4 : c.standing) + ' of ' + (c.squadSize || 4) + ' standing · ' + (c.perks || 0) + (c.perks === 1 ? ' perk' : ' perks') + (c.seed ? ' · seed ' + c.seed : ''))) : null,
      h(VbButton, { title: 'PLAY', variant: 'primary', sub: p.squadLine || '', icon: 'chevron', className: 'vb-menu__play', onClick: call(p.onPlay) }),
      h('div', { className: 'vb-row vb-menu__row' }, h(VbButton, { title: 'HIGH SCORES', onClick: call(p.onScores) }), h(VbButton, { title: 'SETTINGS', icon: 'settings', onClick: call(p.onSettings) })),
      p.notice ? L('vb-t-caption vb-menu__notice', p.notice) : null);
  }
  /** MainMenuView: the offline badge and best, the mark, the menu actions and EXIT. `view`: MainMenuViewModel. */
  function MainMenuView(p) {
    var v = Object.assign({ status: 'OFFLINE · LOCAL SCORES', subtitle: 'ARCADE', notice: '', best: 0, bestWave: 0, checkpoint: null, squad: [], canExit: true }, p.view);
    return View('vb-menu', p, [
      h('div', { key: 't', className: 'vb-menu__top' }, L('vb-t-badge', v.status), v.best > 0 ? h(BadgeComponent, { text: 'BEST', value: num(v.best), className: 'vb-menu__best' }, v.bestWave ? L('vb-t-badge vb-t-ally vb-menu__best-wave', 'WAVE ' + v.bestWave) : null) : null),
      h('div', { key: 'b', className: 'vb-menu__brand' }, h(VbMark, { width: 240 }), v.subtitle ? L('vb-menu__arcade', upper(v.subtitle)) : null),
      h('div', { key: 'g', className: 'vb-grow' }),
      h(MenuActionsSectionView, { key: 'a', checkpoint: v.checkpoint, squadLine: v.squad.length ? 'Squad · ' + v.squad.join(' · ') : 'Choose a squad', notice: v.notice, onPlay: p.onPlay, onResume: p.onResume, onScores: p.onScores, onSettings: p.onSettings }),
      v.canExit ? h(VbButton, { key: 'x', title: 'EXIT', variant: 'ghost', className: 'vb-menu__exit', onClick: call(p.onExit) }) : null]);
  }
  /** SquadView: four slots, the roster, the open details and START RUN (blocked until four). `view`: SquadViewModel. */
  function SquadView(p) {
    var v = Object.assign({ roster: [], selected: [-1, -1, -1, -1], focus: -1, locked: [], tiers: [], seed: 'new seed' }, p.view);
    var picked = v.selected.filter(function (i) { return i >= 0 && i < v.roster.length; }), n = picked.length;
    var unlocked = v.roster.filter(function (_, i) { return !v.locked[i]; }).length, open = v.focus >= 0 && v.focus < v.roster.length;
    var shownFocus = R.useRef(-1); if (open) shownFocus.current = v.focus; var fi = shownFocus.current;  // keep the details while the drawer slides away
    return View('vb-squad-view', p, [
      h(ScreenHeader, { key: 'h', title: 'SQUAD', meta: n + ' / 4', accent: n === 4, onBack: p.onBack }),
      h(LabelComponent, { key: 'l1', text: 'Squad · tap a slot for details', className: 'vb-page__label' }),
      h('div', { key: 's', className: 'vb-grid4' }, [0, 1, 2, 3].map(function (i) { var r = v.selected[i]; return h(SquadSlot, { key: i, slot: i + 1, view: r >= 0 ? v.roster[r] : null, onPick: function () { call(p.onSlotPick)(i); } }); })),
      h(LabelComponent, { key: 'l2', text: unlocked === v.roster.length ? 'Roster · all ' + v.roster.length + ' unlocked' : 'Roster · ' + unlocked + ' of ' + v.roster.length + ' unlocked', className: 'vb-page__label' }),
      h('div', { key: 'r', className: 'vb-grid4 vb-grid4--wrap' }, v.roster.map(function (o, i) { return h(OperativeTile, { key: i, view: o, slot: v.selected.indexOf(i) + 1, focused: i === v.focus, tier: v.tiers[i], locked: v.locked[i], onPick: function () { call(p.onPick)(i); } }); })),
      h('div', { key: 'g', className: 'vb-grow' }),
      h(VbButton, { key: 'go', title: 'START RUN', variant: 'primary', icon: 'chevron', className: 'vb-squad-view__start', sub: n ? picked.map(function (i) { return v.roster[i].name; }).join(' · ') + (v.seed ? ' · ' + v.seed : '') : '', blocked: n < 4 ? 'Pick ' + (4 - n) + ' more' : '', onClick: call(p.onStart) }),
      h(DrawerOverlay, { key: 'd', open: open, closeButton: false, className: 'vb-squad-view__drawer', label: 'Operative details', onDismiss: p.onClose },
        fi >= 0 && v.roster[fi] ? h(OperativeDetails, { view: v.roster[fi], slot: v.selected.indexOf(fi) + 1, squadFull: n >= 4, onClose: p.onClose, onToggle: function () { call(p.onToggle)(v.focus); } }) : null)], true);
  }
  /** SettingsView: AUDIO, FEEL, CAMERA, QUALITY in a FormComposite; applies as it changes (`onChange(model)`). */
  function SettingsView(p) {
    var init = Object.assign({ master: 0.8, music: 0.55, effects: 0.8, sensitivity: 0.5, haptics: true, reducedMotion: false, camera: 0, quality: 1, saved: 'SAVED', note: 'Autoplay lives in the battle HUD and starts on for every new run.' }, p.view);
    var s = R.useState(init), v = s[0];
    function set(k) { return function (x) { var n = Object.assign({}, v); n[k] = x; s[1](n); if (p.onChange) p.onChange(n); }; }
    return View('vb-settings', p, [
      h(ScreenHeader, { key: 'h', title: 'SETTINGS', meta: v.saved, onBack: p.onBack }),
      h(FormComposite, { key: 'f', headed: true, note: v.note ? { icon: 'automation', text: v.note } : null,
        footer: [h(VbButton, { key: 'c', title: 'CLEAR LOCAL SCORES', variant: 'danger', onClick: call(p.onClearScores) }), h(VbButton, { key: 'a', title: 'ACCESSIBILITY', variant: 'ghost', onClick: call(p.onAccessibility) })] },
        h(Fieldset, { key: 1, title: 'AUDIO' }, h(VbSlider, { label: 'Master', value: v.master, onChange: set('master') }), h(VbSlider, { label: 'Music', value: v.music, onChange: set('music') }), h(VbSlider, { label: 'Effects', value: v.effects, onChange: set('effects') })),
        h(Fieldset, { key: 2, title: 'FEEL' }, h(VbSwitch, { text: 'HAPTICS', detail: 'Light pulse when an order is accepted', value: v.haptics, onChange: set('haptics') }), h(VbSwitch, { text: 'REDUCED MOTION', detail: 'No camera shake · fades replace flicker · flat panels', value: v.reducedMotion, onChange: set('reducedMotion') })),
        h(Fieldset, { key: 3, title: 'CAMERA' }, h(VbSegmented, { items: [{ label: 'FULL', sub: 'Parallax' }, { label: 'REDUCED', sub: 'No drift' }, { label: 'OFF', sub: 'Fixed' }], selected: v.camera, onChange: set('camera') }), h(VbSlider, { label: 'Sensitivity', value: v.sensitivity, onChange: set('sensitivity') }), L('vb-t-caption', 'Pinch to zoom, twist to turn, drag empty ground to pan, double-tap to reset.')),
        h(Fieldset, { key: 4, title: 'QUALITY' }, h(VbSegmented, { items: [{ label: 'LOW', sub: 'Stable 30 FPS' }, { label: 'STANDARD', sub: '60 FPS target' }], selected: v.quality, onChange: set('quality') }), L('vb-t-caption', 'Applies on the next wave. Telegraphs stay fully visible on both tiers.')))], true);
  }
  /** PauseOverlayView: a ScreenHeader whose back square resumes, then one overlay panel centred on the screen. `view`: PauseViewModel. */
  function PauseOverlayView(p) {
    var v = Object.assign({ wave: 1, score: 0, standing: 4, squadSize: 4, perks: [], seed: '', detail: '' }, p.view);
    var line = [v.seed ? 'Seed ' + v.seed : '', v.detail].filter(Boolean).join(' · ');
    return View('vb-pause', p, [
      h(ScreenHeader, { key: 'h', title: 'PAUSED', meta: 'WAVE ' + pad2(v.wave), backLabel: 'Resume', onBack: p.onResume }),
      h('div', { key: 'c', className: 'vb-pause__center' }, L('vb-t-caption vb-pause__caption', 'Combat, focus and world travel are frozen'),
        h(Dialog, { className: 'vb-pause__panel' },
          h('div', { className: 'vb-row vb-pause__stats' }, h(StatReadout, { label: 'WAVE', value: pad2(v.wave) }), h(StatReadout, { label: 'SCORE', value: num(v.score) }), h(StatReadout, { label: 'STANDING', value: String(v.standing), unit: '/' + v.squadSize })),
          v.perks.length ? h(Shape, Object.assign({ className: 'vb-panel vb-frame--slim vb-frame--raised vb-pause__perks', title: 'Active perks' }, clickable(p.onPerks)),
            h('div', { className: 'vb-row vb-row--between' }, h(LabelComponent, { text: 'Active perks · ' + v.perks.length }), h('div', { className: 'vb-row' }, L('vb-t-badge vb-t-ally', 'DETAILS'), h(VbIcon, { icon: 'chevron', size: 16, className: 'vb-pause__more' }))),
            h('div', { className: 'vb-row vb-row--wrap vb-pause__chips' }, v.perks.map(function (k, i) { return h(BadgeComponent, { key: i, icon: k.icon, text: k.name + (k.stack > 1 ? ' ×' + k.stack : ''), className: 'vb-pause__chip' }); }))) : null,
          line ? L('vb-t-caption', line) : null,
          h(VbButton, { title: 'RESUME', variant: 'primary', sub: 'Returns to selected speed', icon: 'chevron', onClick: call(p.onResume) }),
          h('div', { className: 'vb-row vb-pause__row' }, h(VbButton, { title: 'SETTINGS', icon: 'settings', className: 'vb-half vb-pause__button', onClick: call(p.onSettings) }),
            h(VbButton, { title: 'ABANDON', variant: 'danger', icon: 'remove', className: 'vb-half vb-pause__button vb-pause__abandon', onClick: call(p.onAbandon) })),
          h(VbButton, { title: 'EXIT TO MENU', variant: 'ghost', icon: 'back', className: 'vb-pause__exit', onClick: call(p.onExit) })))], true);
  }
  /** ConfirmDialogView: the question, its consequence, the destructive action and the way back. */
  function ConfirmDialogView(p) {
    var c = p.confirm || {}, k = p.cancel || {};
    return h('div', { className: cx('vb-confirm', p.className), style: p.style }, h(Dialog, { title: p.title, body: p.body },
      h(VbButton, { title: c.label || 'CONFIRM', variant: c.variant || 'danger', sub: c.sub, icon: c.icon, onClick: call(p.onConfirm) }),
      h(VbButton, { title: k.label || 'CANCEL', icon: k.icon, onClick: call(p.onCancel) })));
  }
  var GUIDE_STEPS = ['Drag an operative to move.', 'Drag A or S onto the battlefield to aim; release over the cards to cancel.', 'Auto controls tactics. Tap AUTO to take over.', '2× speeds combat; aiming stays slow while focus lasts.', 'Stop enemies before they pass the rear line.'];
  /** GuideDialogView: numbered first-run steps and READY. */
  function GuideDialogView(p) {
    return h('div', { className: cx('vb-guide', p.className), style: p.style }, h(Dialog, { title: p.title || 'RUN THE BOULEVARD' },
      h(StackLayout, { gap: 8, className: 'vb-guide__steps' }, (p.steps || GUIDE_STEPS).map(function (t, i) { return h('div', { key: i, className: 'vb-row vb-guide__step' }, h(BadgeComponent, { text: String(i + 1), tone: 'ally', className: 'vb-guide__index' }), L('vb-t-body vb-guide__text', t)); })),
      h(VbButton, { title: 'READY', variant: 'primary', icon: 'check', onClick: call(p.onReady) })));
  }
  /** PerkDraftView: three cards, the pick explained, the squad that carries forward and TAKE PERK. `view`: PerkDraftViewModel. */
  function PerkDraftView(p) {
    var v = Object.assign({ wave: 1, bonus: 0, score: 0, choices: [], pick: 0, squad: [] }, p.view), pick = v.choices[v.pick];
    return View('vb-draft', p, [
      h('div', { key: 's', className: 'vb-page__scroll vb-draft__scroll', style: { overflowY: 'auto' } },
        h(LabelComponent, { text: 'Wave ' + pad2(v.wave) + ' cleared', tone: 'ally' }), L('vb-t-display', 'CHOOSE A PERK'),
        L('vb-t-caption vb-page__caption', '+' + num(v.bonus) + ' wave bonus · score ' + num(v.score) + ' · perks last the whole run'),
        h('div', { className: 'vb-draft__cards' }, v.choices.map(function (c, i) { return h(PerkCard, { key: i, view: c, selected: i === v.pick, onPick: function () { call(p.onPick)(i); } }); })),
        pick ? h(PerkDetail, { className: 'vb-draft__detail', view: pick, draft: true }) : null,
        h(Shape, { className: 'vb-panel vb-frame--slim vb-draft__squad' }, h(LabelComponent, { text: 'Carries forward · no healing between waves', size: 'badge' }),
          h('div', { className: 'vb-grid4' }, v.squad.map(function (a, i) {
            return h('div', { key: i, className: 'vb-draft__cell' }, h('div', { className: 'vb-row vb-row--between' }, L('vb-draft__code', a.code), L('vb-t-badge', a.hp > 0 ? Math.round(100 * a.hp / Math.max(1, a.maxHp)) + '%' : 'OUT')),
              h(HealthBar, { hp: a.hp, max: a.maxHp, stability: a.stability == null ? 1 : a.stability }));
          })))),
      h(VbButton, { key: 'b', title: 'TAKE PERK', variant: 'primary', icon: 'check', sub: pick ? pick.name + ' → ' + pick.next + ' · then wave ' + pad2(v.wave + 1) + ', focus refills' : '', blocked: pick ? '' : 'Pick a perk', onClick: call(p.onTake) })]);
  }
  /** ResultsView: the score and its ledger, stats, the squad's contribution, the perks, MENU and RUN AGAIN. `view`: ResultsViewModel. */
  function ResultsView(p) {
    var v = Object.assign({ cause: 'SQUAD DOWN', score: 0, wave: 0, bosses: 0, rank: 0, of: 20, ledger: [], distance: '', simTime: '', squad: [], perks: [] }, p.view), d = (v.distance || '').split(' ');
    return View('vb-results', p, [
      h('div', { key: 's', className: 'vb-page__scroll vb-results__scroll', style: { overflowY: 'auto' } },
        h(LabelComponent, { text: v.cause + ' · wave ' + v.wave, tone: 'enemy' }), L('vb-t-display', 'RUN OVER'),
        h(Shape, { className: 'vb-panel vb-frame--slim vb-frame--solid vb-results__score' },
          h('div', { className: 'vb-row vb-row--between' }, h('div', null, h(LabelComponent, { text: 'Score' }), L('vb-results__value', num(v.score))), v.rank > 0 ? L('vb-results__rank', '#' + v.rank + ' OF ' + v.of) : null),
          h('div', { className: 'vb-results__ledger' }, v.ledger.map(function (l, i) { return h('div', { key: i, className: 'vb-row vb-row--between' }, L('vb-t-caption', l.label), L('vb-results__points', l.value)); })),
          h('div', { className: 'vb-row vb-results__stats' }, h(StatReadout, { label: 'WAVES', value: String(v.wave) }), h(StatReadout, { label: 'BOSSES', value: String(v.bosses) }),
            v.distance ? h(StatReadout, { label: 'DISTANCE', value: d[0], unit: d[1] }) : null, v.simTime ? h(StatReadout, { label: 'SIM TIME', value: v.simTime }) : null)),
        h(LabelComponent, { text: 'Squad · hover or tap for contribution', className: 'vb-page__label' }),
        h('div', { className: 'vb-grid4' }, v.squad.map(function (c, i) { return h(SquadResult, { key: i, view: c }); })),
        v.perks.length ? h(LabelComponent, { text: 'Perks · ' + v.perks.length, className: 'vb-page__label' }) : null,
        h('div', { className: 'vb-row vb-row--wrap vb-results__perks' }, v.perks.map(function (k, i) { return h(PerkChip, { key: i, view: k }); }))),
      h('div', { key: 'r', className: 'vb-row vb-results__row' }, h(VbButton, { title: 'MENU', className: 'vb-results__menu', onClick: call(p.onMenu) }), h(VbButton, { title: 'RUN AGAIN', variant: 'primary', icon: 'chevron', className: 'vb-results__again', onClick: call(p.onAgain) }))]);
  }
  /** HighScoresView: up to 20 ranked runs (current framed) and older-rules runs after a label, in a ListLayout. */
  function HighScoresView(p) {
    var ranked = (p.ranked || []).slice(0, 20), old = p.historical || [];
    return View('vb-scores', p, [
      h(ScreenHeader, { key: 'h', title: 'HIGH SCORES', meta: 'TOP ' + ranked.length, onBack: p.onBack }),
      h('div', { key: 'c', className: 'vb-l vb-t-caption vb-page__caption' }, p.caption || 'On this device. Ties break by wave, then shortest sim time.'),
      h(ListLayout, { key: 'l', empty: 'No runs yet. Your first run lands here.' }, ranked.map(function (s, i) { return h(ScoreRow, { key: 'r' + i, view: s }); }),
        old.length ? h(LabelComponent, { key: 'o', text: 'Older rules', className: 'vb-scores__older' }) : null,
        old.map(function (s, i) { return h(ScoreRow, { key: 'h' + i, view: s, className: 'vb-scores__historical' }); })),
      h('div', { key: 'f', className: 'vb-l vb-t-caption vb-page__footer' }, 'Local scores only · not an online ranking')], true);
  }
  function RouteChoiceView(p) { return View('vb-route-view', p, h(RouteChoice, { view: p.view, onChoose: p.onChoose })); }
  function MerchantScreenView(p) { return View('vb-merchant-view', p, h(MerchantScreen, { view: p.view, onBuy: p.onBuy, onSellPerk: p.onSellPerk, onSellHealth: p.onSellHealth, onLeave: p.onLeave })); }
  /** BattleHudView: BoardLayout with the HUD band, boss bar and announcer on top, `world` in the middle and four cards at the bottom. */
  function BattleHudView(p) {
    var ops = p.operatives || [];
    return h(BoardLayout, { className: cx('vb-battle', p.className), style: p.style,
      top: [h(HudTop, { key: 'h', view: p.hud, onPauseClick: p.onPause, onAutoClick: p.onAuto, onSpeedClick: p.onSpeed, onQueueCancel: p.onQueueCancel, queuePinned: p.queuePinned }),
        p.boss ? h(BossBar, { key: 'b', view: p.boss, className: 'vb-battle__boss' }) : null,
        h('div', { key: 'a', className: 'vb-battle__announcer vb-announcement' }, p.announcement ? h(GlitchText, { key: p.announcement, text: p.announcement }) : null)],
      world: p.world,
      bottom: h('div', { className: 'vb-battle__cards' }, ops.slice(0, 4).map(function (o, i) { return o ? h(OperativeCard, { key: i, view: o, className: 'vb-battle__card' }) : null; })) });
  }

  var api = {
    GlitchText: GlitchText, CreditsChip: CreditsChip, RouteChoice: RouteChoice, MerchantScreen: MerchantScreen,
    VbFrame: VbFrame, VbIcon: VbIcon, VbMark: VbMark, VbButton: VbButton, StatReadout: StatReadout, StatusChip: StatusChip,
    VbSwitch: VbSwitch, VbSlider: VbSlider, VbSegmented: VbSegmented, VbMeter: VbMeter, HealthBar: HealthBar, PowerStrip: PowerStrip,
    FocusMeter: FocusMeter, SkillButton: SkillButton, QueueChip: QueueChip, HudTop: HudTop, QueueList: QueueList, BossBar: BossBar, ScreenHeader: ScreenHeader,
    ScoreRow: ScoreRow, OperativeCard: OperativeCard, OperativeTile: OperativeTile, SquadSlot: SquadSlot, OperativeDetails: OperativeDetails,
    OperativeStats: OperativeStats, SquadResult: SquadResult, PerkCard: PerkCard, PerkDetail: PerkDetail, PerkChip: PerkChip, PerkRow: PerkRow,
    Dialog: Dialog, Backdrop: Backdrop, ScanLines: ScanLines, HoloFrame: HoloFrame, HoloPlatform: HoloPlatform, RevealCard: RevealCard, RuleRow: RuleRow,
    WorldHealthBar: WorldHealthBar, CombatText: CombatText, WorldLabel: WorldLabel, WorldMarker: WorldMarker, OffscreenIndicator: OffscreenIndicator, EdgeWarning: EdgeWarning,
    ProgressBar: ProgressBar, ListRow: ListRow, TextComponent: TextComponent, LabelComponent: LabelComponent, BadgeComponent: BadgeComponent, IconButtonComposite: IconButtonComposite, TileComponent: TileComponent, CardComponent: CardComponent,
    DrawerOverlay: DrawerOverlay, FieldComposite: FieldComposite, Fieldset: Fieldset, FormComposite: FormComposite, StackLayout: StackLayout, VbGrid: VbGrid, ListLayout: ListLayout, BoardLayout: BoardLayout,
    MainMenuView: MainMenuView, MenuActionsSectionView: MenuActionsSectionView, SquadView: SquadView, SettingsView: SettingsView, PauseOverlayView: PauseOverlayView, ConfirmDialogView: ConfirmDialogView,
    GuideDialogView: GuideDialogView, PerkDraftView: PerkDraftView, ResultsView: ResultsView, HighScoresView: HighScoresView, RouteChoiceView: RouteChoiceView, MerchantScreenView: MerchantScreenView, BattleHudView: BattleHudView,
    VbShape: Shape, format: { compact: compact, number: num, seconds: seconds, kebab: kebab }
  };
  window.Veilbreak = Object.assign(window.Veilbreak || {}, api);
})(window);
export const Veilbreak = window.Veilbreak;
export default Veilbreak;
export const GlitchText = Veilbreak.GlitchText;
export const CreditsChip = Veilbreak.CreditsChip;
export const RouteChoice = Veilbreak.RouteChoice;
export const MerchantScreen = Veilbreak.MerchantScreen;
export const VbFrame = Veilbreak.VbFrame;
export const VbIcon = Veilbreak.VbIcon;
export const VbMark = Veilbreak.VbMark;
export const VbButton = Veilbreak.VbButton;
export const StatReadout = Veilbreak.StatReadout;
export const StatusChip = Veilbreak.StatusChip;
export const VbSwitch = Veilbreak.VbSwitch;
export const VbSlider = Veilbreak.VbSlider;
export const VbSegmented = Veilbreak.VbSegmented;
export const VbMeter = Veilbreak.VbMeter;
export const HealthBar = Veilbreak.HealthBar;
export const PowerStrip = Veilbreak.PowerStrip;
export const FocusMeter = Veilbreak.FocusMeter;
export const SkillButton = Veilbreak.SkillButton;
export const QueueChip = Veilbreak.QueueChip;
export const HudTop = Veilbreak.HudTop;
export const QueueList = Veilbreak.QueueList;
export const BossBar = Veilbreak.BossBar;
export const ScreenHeader = Veilbreak.ScreenHeader;
export const ScoreRow = Veilbreak.ScoreRow;
export const OperativeCard = Veilbreak.OperativeCard;
export const OperativeTile = Veilbreak.OperativeTile;
export const SquadSlot = Veilbreak.SquadSlot;
export const OperativeDetails = Veilbreak.OperativeDetails;
export const OperativeStats = Veilbreak.OperativeStats;
export const SquadResult = Veilbreak.SquadResult;
export const PerkCard = Veilbreak.PerkCard;
export const PerkDetail = Veilbreak.PerkDetail;
export const PerkChip = Veilbreak.PerkChip;
export const PerkRow = Veilbreak.PerkRow;
export const Dialog = Veilbreak.Dialog;
export const Backdrop = Veilbreak.Backdrop;
export const ScanLines = Veilbreak.ScanLines;
export const HoloFrame = Veilbreak.HoloFrame;
export const HoloPlatform = Veilbreak.HoloPlatform;
export const RevealCard = Veilbreak.RevealCard;
export const RuleRow = Veilbreak.RuleRow;
export const WorldHealthBar = Veilbreak.WorldHealthBar;
export const CombatText = Veilbreak.CombatText;
export const WorldLabel = Veilbreak.WorldLabel;
export const WorldMarker = Veilbreak.WorldMarker;
export const OffscreenIndicator = Veilbreak.OffscreenIndicator;
export const EdgeWarning = Veilbreak.EdgeWarning;
export const ProgressBar = Veilbreak.ProgressBar;
export const ListRow = Veilbreak.ListRow;
export const TextComponent = Veilbreak.TextComponent;
export const LabelComponent = Veilbreak.LabelComponent;
export const BadgeComponent = Veilbreak.BadgeComponent;
export const IconButtonComposite = Veilbreak.IconButtonComposite;
export const TileComponent = Veilbreak.TileComponent;
export const CardComponent = Veilbreak.CardComponent;
export const DrawerOverlay = Veilbreak.DrawerOverlay;
export const FieldComposite = Veilbreak.FieldComposite;
export const Fieldset = Veilbreak.Fieldset;
export const FormComposite = Veilbreak.FormComposite;
export const StackLayout = Veilbreak.StackLayout;
export const VbGrid = Veilbreak.VbGrid;
export const ListLayout = Veilbreak.ListLayout;
export const BoardLayout = Veilbreak.BoardLayout;
export const MainMenuView = Veilbreak.MainMenuView;
export const MenuActionsSectionView = Veilbreak.MenuActionsSectionView;
export const SquadView = Veilbreak.SquadView;
export const SettingsView = Veilbreak.SettingsView;
export const PauseOverlayView = Veilbreak.PauseOverlayView;
export const ConfirmDialogView = Veilbreak.ConfirmDialogView;
export const GuideDialogView = Veilbreak.GuideDialogView;
export const PerkDraftView = Veilbreak.PerkDraftView;
export const ResultsView = Veilbreak.ResultsView;
export const HighScoresView = Veilbreak.HighScoresView;
export const RouteChoiceView = Veilbreak.RouteChoiceView;
export const MerchantScreenView = Veilbreak.MerchantScreenView;
export const BattleHudView = Veilbreak.BattleHudView;
export const VbShape = Veilbreak.VbShape;
export const format = Veilbreak.format;
