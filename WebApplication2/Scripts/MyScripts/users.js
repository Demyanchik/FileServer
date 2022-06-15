function DeleteUserShow(login)
{
    $('#main_menu').css('z-index', '0');

    $('#delete_login').val(login);
    $('#delete_login_span').html(login);

}

function DeleteUserClose() {
    $('#main_menu').css('z-index', '1030');
}