var app = function() {

    $(function() {
        navToggleRight();
        navToggleLeft();
        navToggleSub();
        profileToggle();


    });


    var navToggleRight = function() {
        $('#toggle-right').on('click', function() {
            $('#sidebar-right').toggleClass('sidebar-right-open');
            $("#toggle-right .fa").toggleClass("fa-indent fa-dedent");

        });
    };

   

    var navToggleLeft = function() {
        $('#toggle-left').on('click', function() {
            var bodyEl = $('#main-wrapper');
            ($(window).width() > 767) ? $(bodyEl).toggleClass('sidebar-mini'): $(bodyEl).toggleClass('sidebar-opened');
        });
    };

    var navToggleSub = function() {
        var subMenu = $('.sidebar .nav');
        $(subMenu).navgoco({
            caretHtml: false,
            accordion: true
        });

    };

    var profileToggle = function() {
        $('#toggle-profile').click(function() {
            $('.sidebar-profile').slideToggle();
        });
    };


    //return functions
    return {

    };
}();


