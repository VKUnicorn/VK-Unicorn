// Возвращает слово в зависимости от числа
// 21 год
// 22 года
// 27 лет
function one_few_many(number, one, few, many)
{
    number = Math.abs(number);
    var mod100 = number % 100;

    var result = many;
    if (!(mod100 >= 11 && mod100 <= 19))
    {
        switch (number % 10)
        {
            case 1:
                result = one;
                break;

            case 2:
            case 3:
            case 4:
                result = few;
                break;
        }
    }

    return result;
}

function clear_workspace() {
    $('#workspace').empty()
}

function start_loading() {
    $('#loading').show()
}

function finish_loading() {
    $('#loading').fadeOut()
}

function main() {
    // Добавляем поддержку русского языка в bootbox
    var locale = {
        OK: 'Ок',
        CONFIRM: 'Да',
        CANCEL: 'Отмена'
    };

    bootbox.addLocale('russian', locale);

    // Создаём и настраиваем плагин уведомлений
    $.hulla = new hullabaloo();
    $.hulla.options.delay = 2000;
    $.hulla.options.width = 250;
    $.hulla.options.offset.from = "bottom";
    $.hulla.options.allow_dismiss = false;
    $.hulla.options.alertClass = "settings-form-font";

    start_loading();
}