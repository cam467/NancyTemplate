//All App related code goes here
var App = (function() {

    var _url = '/api/',

        startUp = function() {
            //Document event binding
            $(document).on('click','#apitestsettings',function() {
                $.ajax({
                    type: 'GET',
                    url: _url + 'testsettings',
                    dataType: 'html',
                    beforeSend: function() {
                        $('div.feedback').text('Running Test...');
                    },
                    success: function(data) {
                        console.log(data);
                        $('div.feedback').removeClass('error success').addClass(data.toLowerCase().indexOf('failed') > -1 ? 'error' : 'success').text(data);
                    }
                });
            });
        };

    return {
        init: startUp
    };
})();