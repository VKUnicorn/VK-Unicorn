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

// Возвращает время переведённое в год как число
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

// Возвращает разницу во времени с датой как число
function isoTimeToLocalDaysDelta(isoTimeAsString) {
    if (isoTimeAsString === undefined) {
        return -1;
    }

    var isoTime = Date.parse(isoTimeAsString);
    if (isoTime < 0)
    {
        return -1;
    }

    var today = new Date();
    var targetDate = new Date(isoTime);
    var differenceInTime = today.getTime() - targetDate.getTime();
    return differenceInTime / (1000 * 3600 * 24);
}

// Возвращает разницу во времени с датой как текст
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
            return 'меньше минуты';
        }

        return oneFewMany(delta, 'минуту', 'минуты', 'минут', true);
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

// Возвращает предупреждение о том, что пользователь ещё не достиг возраста сексуального согласия
function getNotInConsentAgeWarning() {
    return `
        Пользователь не достиг возраста сексуального согласия
        <br>
        <a href="https://ru.wikisource.org/wiki/Уголовный_кодекс_Российской_Федерации/Глава_18#Статья_134" target="_blank">
            <font color=red>
                УК РФ Статья 134. Половое сношение и иные действия сексуального характера с лицом, не достигшим шестнадцатилетнего возраста
            </font>
        </a>
        <br>
        <a href="https://ru.wikisource.org/wiki/Уголовный_кодекс_Российской_Федерации/Глава_18#Статья_135" target="_blank">
            <font color=red>
                УК РФ Статья 135. Развратные действия
            </font>
        </a>
    `;
}

// Возвращает предупреждение о том, что пользователь ещё не достиг совершеннолетия
function getUnderageWarning() {
    return `
        Пользователь не достиг совершеннолетия
        <br>
        <a href="https://ru.wikisource.org/wiki/Уголовный_кодекс_Российской_Федерации/Глава_25#Статья_240.1" target="_blank">
            <font color=red>
                УК РФ Статья 240.1. Получение сексуальных услуг несовершеннолетнего
            </font>
        </a>
    `;
}

// Чинит внешние ссылки без http
function fixExternalUrl(url) {
    if (!url.startsWith("http")) {
        url = "http://" + url;
    }

    return url;
}

// Функция для добавления кнопке срабатывания по зажатию
(function($) {
    var defaultSettings = {
        NS: 'jquery.longclick-',
        delay: 350
    };

    $.fn.mayTriggerLongClicks = function(userSettings) {
        var settings = $.extend(defaultSettings, userSettings);
        var timer;
        var haveLongClick;

        return $(this).on('mousedown', function() {
            haveLongClick = false;

            timer = setTimeout(function(element) {
                haveLongClick = true;

                $(element).trigger('longClick');

                // Отключаем возможность любого выделения пока пользователь не отожмёт кнопку мыши
                toggleAllSelection(false);
            }, settings.delay, this);
        }).on('mouseup', function() {
            clearTimeout(timer);
        }).on('click', function(event) {
            if (haveLongClick) {
                event.stopImmediatePropagation();
            }
        });
    }
})(jQuery);

// Функция для добавления своих классов к popover
function getPopoverTemplateWithClass(customClass, bodyClass) {
    return '<div class="popover ' + customClass + '" role="tooltip"><div class="arrow"></div><h3 class="popover-header"></h3><div class="popover-body ' + bodyClass + '"></div></div>';
}

// Отключает возможность любого выделения в документе
function toggleAllSelection(isEnabled) {
    document.onselectstart = () => {
        return isEnabled;
    };
}

// Восстанавливаем возможность любого выделения в документе когда пользователь отжимает кнопку мыши
document.onmouseup = (e) => {
    toggleAllSelection(true);
};