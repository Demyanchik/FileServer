function switch_tab(tab_name)
{
    hide_all();

    $('#share_button_' + tab_name).addClass('active');
    $('#share_body_' + tab_name).show();
}

function hide_all()
{
    $('#share_button_user').removeClass('active');
    $('#share_button_group').removeClass('active');
    $('#share_button_link').removeClass('active');

    $('#share_body_user').hide();
    $('#share_body_group').hide();
    $('#share_body_link').hide();
}