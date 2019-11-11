// Возвращает слово в зависимости от числа
// 21 год
// 22 года
// 27 лет
function oneFewMany(number, one, few, many, appendNumber)
{
    number = Math.abs(number);
    var mod100 = number % 100;

    var result = appendNumber ? number + ' ' : '';
    if (!(mod100 >= 11 && mod100 <= 19))
    {
        switch (number % 10)
        {
            case 1:
                return result + one;

            case 2:
            case 3:
            case 4:
                return result + few;
        }
    }

    return result + many;
}

function isoTimeToAge(isoTimeAsString) {
    var isoTime = Date.parse(isoTimeAsString);
    if (isoTime < 0)
    {
        return 0;
    }

    var today = new Date();
    var birthDate = new Date(isoTime);
    var age = today.getFullYear() - birthDate.getFullYear();
    var m = today.getMonth() - birthDate.getMonth();
    if (m < 0 || (m === 0 && today.getDate() < birthDate.getDate())) {
        age--;
    }
    return age;
}

function isoTimeToLocalDeltaAsString(isoTimeAsString) {
    if (isoTimeAsString === undefined) {
        return 'никогда';
    }

    var isoTime = Date.parse(isoTimeAsString);
    if (isoTime < 0)
    {
        return 'никогда';
    }

    var delta = parseInt((new Date().getTime() - isoTime) / 1000 / 60);
    if (delta < 60) {
        if (delta == 0) {
            return 'только что';
        }

        return oneFewMany(delta, 'минута', 'минуты', 'минут', true);
    }

    delta = parseInt(delta / 60)
    if (delta < 24) {
        return oneFewMany(delta, 'час', 'часа', 'часов', true);
    }

    delta = parseInt(delta / 24);
    return oneFewMany(delta, 'день', 'дня', 'дней', true);
}

// Функция для обновления счётчика у баджа категории
function updateBadgeRelative(category, value) {
    let categoryElement = $(category);
    let badgeElement = categoryElement.find('#badge');

    let oldValue = parseInt(badgeElement.text());
    let newValue = oldValue + value;

    badgeElement.text(newValue);

    if (newValue > 0) {
        categoryElement.show();
    } else {
        categoryElement.hide();
    }
}

// Функция для добавления кнопке срабатывания по зажатию
(function($) {
    var defaults = {
        NS: 'jquery.longclick-',
        delay: 500
    };

    $.fn.mayTriggerLongClicks = function(options) {
        var settings = $.extend(defaults, options);
        var timer;
        var haveLongClick;

        return $(this).on('mousedown', function() {
            haveLongClick = false;

            timer = setTimeout(function(elm) {
                haveLongClick = true;

                $(elm).trigger('longClick');
            }, settings.delay, this);
        }).on('mouseup', function() {
            clearTimeout(timer);
        }).on('click', function(evt) {
            if (haveLongClick) {
                evt.stopImmediatePropagation();
            }
        });
    }
})(jQuery);

// Функция для добавления своих классов к popover
function getPopoverTemplateWithClass(customClass) {
    return '<div class="popover ' + customClass + '" role="tooltip"><div class="arrow"></div><h3 class="popover-header"></h3><div class="popover-body"></div></div>';
}