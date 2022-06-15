function item_click(file_name, dir, type, path) {
    $('#context_menu_name').val(dir);
    $('#context_menu_type').val(type);
    $('#file_partial_path').val(path);

    if (($("#file_name").html() == file_name) && ($("#file_menu").is(":visible"))) {
        if (type == "Folder")
            go_to_directory(dir);
        else
            download();
    }
    else {
        show_file_menu(file_name);
    }

}

function groups_item_click(file_name, dir, type, path, query) {
    $('#context_menu_name').val(dir);
    $('#context_menu_type').val(type);
    $('#file_partial_path').val(path);

    if (($("#file_name").html() == file_name) && ($("#file_menu").is(":visible"))) {
        if (type == "Folder")
            groups_go_to_directory(dir, query);
        else
            download();
    }
    else {
        show_file_menu(file_name);
    }

}

function set_link(link)
{
    $('#shared_link_value').val(link);
}

function link_modal(user, path, type)
{
    $('#link_modal_user').val(user);
    $('#link_modal_path').val(path);
    $('#link_modal_type').val(type);
}

function show_file_menu(file_name)
{
    $('#file_menu').show(); $('#main_menu').hide();

    $("#file_name").html(file_name);
}

function file_info(file_name, file_size, last_changed)
{
    $("#info_file_name").html(file_name);
    $("#info_file_size").html(file_size);
    $("#info_last_changed").html(last_changed);
}

function share_click(user, path)
{
    $.get('http://localhost:44318/Home/GenerateLink', { user: user, path: path }, function (data) {
        $('#link_value').val(data);
    });

}

function go_to_directory(dir)
{
    document.location.href += dir + '/';
}

function groups_go_to_directory(dir, query) {
    if (!query.includes('catchall'))
    {
        document.location.href += '&catchall=' + dir + '/';
    }
    else
        document.location.href += dir + '/';
}

function remove()
{
    $('#file_context_menu').attr('action', '/Home/RemoveFile/');
    $('#context_menu_submit').click();
}

function rename() {
    $('#file_context_menu').attr('action', '/Home/Rename/');
    $('#context_menu_new_name').val($('#new_name').val());
    $('#context_menu_submit').click();
}

function putSlash(id) {
    $('#' + id).val($('#' + id).val() + '/');
}

function moveTo() {
    $('#file_context_menu').attr('action', '/Home/MoveTo/');
    $('#context_menu_src_dir').val($('#move_src_path').val());
    $('#context_menu_submit').click();
}

function copyTo() {
    $('#file_context_menu').attr('action', '/Home/CopyTo/');
    $('#context_menu_src_dir').val($('#copy_src_path').val());
    $('#context_menu_submit').click();
}

function remove_permanent() {
    $('#file_context_menu').attr('action', '/Home/RemovePermanent/');
    $('#context_menu_submit').click();
}

function restore() {
    $('#file_context_menu').attr('action', '/Home/Restore/');
    $('#context_menu_submit').click();
}

function download()
{
    $('#file_context_menu').attr('action', '/Home/DownloadFile/');
    $('#context_menu_submit').click();
}

function file_menu_background() {
    $('#main_menu').css('z-index', '0');
}

function file_menu_foreground() {
    $('#main_menu').css('z-index', '1030');
}

function go_to_a(group) {
    document.location = document.location + '?root=' + group + '&catchall=';
}