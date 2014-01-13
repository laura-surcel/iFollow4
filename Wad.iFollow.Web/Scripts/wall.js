function mainpage() { }

$(document).ready(function () {
    $("#SearchBox").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: "/Wall/AutoCompleteSearch",
                type: "POST",
                dataType: "json",
                data: { term: request.term },
                success: function (data) {
                    response($.map(data, function (item) {
                        var autocomplete = $(".ui-autocomplete");
                        var oldLeft = $('#searchSuggest').offset().left;
                        var oldTop = $('#searchSuggest').offset().top;

                        var newTop = oldTop + $("#utility-bar").height() - 4;
                        autocomplete.css("top", newTop);
                        autocomplete.css("left", oldLeft);

                        return { label: item.id, value: item.firstName };
                    }))
                }
            })
        },
        html: true,
        select: function (event, ui) {
            $('#search').val(ui.item.label);
            $('#Id').val(ui.item.value);
            return true;
        },
        minLength: 1
    });
})

$(document).ready(function () {
    //we bind only to the rateit controls within the products div
    debugger;
    $('#rating .rateit').bind('rated reset', function (e) {
        var ri = $(this);
      
        //if the use pressed reset, it will get value: 0 (to be compatible with the HTML range control), we could check if e.type == 'reset',
        //and then set the value to  null .
        var value = ri.rateit('value');
        var id = ri.data('ratingid');

        ri.rateit('readonly', true);
 
        $.ajax({
            url: '/Wall/Rate', //your server side script
            data: {id:id, value: value }, //our data
            type: 'POST',
            success: function (data) {
                alert("your data has been submited")
 
            },
            error: function (jxhr, msg, err) {
                $('#response').append('<li style="color:red">' + msg + '</li>');
            }
        });
    });
})