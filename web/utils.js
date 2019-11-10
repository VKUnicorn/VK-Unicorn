// Возвращает слово в зависимости от числа
// 21 год
// 22 года
// 27 лет
function oneFewMany(number, one, few, many)
{
    number = Math.abs(number);
    var mod100 = number % 100;

    if (!(mod100 >= 11 && mod100 <= 19))
    {
        switch (number % 10)
        {
            case 1:
                return one;

            case 2:
            case 3:
            case 4:
                return few;
        }
    }

    return many;
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

        return delta + ' ' + oneFewMany(delta, 'минута', 'минуты', 'минут');
    }

    delta = parseInt(delta / 60)
    if (delta < 24) {
        return delta + ' ' + oneFewMany(delta, 'час', 'часа', 'часов');
    }

    delta = parseInt(delta / 24);
    return delta + ' ' + oneFewMany(delta, 'день', 'дня', 'дней');
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