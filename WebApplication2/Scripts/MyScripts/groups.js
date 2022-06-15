function CheckGroups(group, invalid, disable) {
    $.get('http://localhost:44318/Home/CheckGroups', { group_name: group }, function (data) {

        if (data < 0) {
            $(invalid).addClass('is-invalid');
            $(invalid).attr('title', 'Группа с таким названием уже существует.');
            $(disable).addClass('disabled');
        }
        else {
            $(invalid).removeClass('is-invalid');
            $(invalid).removeAttr('title');
            $(disable).removeClass('disabled');
        }
    });
}

function DeleteGroup(group_name, id) {
    if (confirm("Вы действительно хотите удалить группу «" + group_name + "»? Все файлы группы будут удалены и недоступны для ее участников."))
    {
        $(id).
        $(id).submit();
    }
}

function SubmitCheck(form, group_name)
{
    if (form.submited === 'Сохранить изменения') {
        form.attr('action', '/Home/UpdateGroup/');
        form.submit();
    }
    else
    {
        if (confirm("Вы действительно хотите удалить группу «" + group_name + "»? Все файлы группы будут удалены и недоступны для ее участников."))
        {
            var id = '#' + form.id;
            $(id).attr('action', '/Home/DeleteGroup/');
            form.submit();
        }
    }
}