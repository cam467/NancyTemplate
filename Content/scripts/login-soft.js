var Login = function () {

    var CreateAccount = function () {
        var userdata = {
            store: $("#selectstore").val(),
            username: $("#newusername").val(),
            firstname: $("#firstname").val(),
            lastname: $("#lastname").val(),
            password: $("#register_password").val(),
            email: $("#email").val(),
            phone: $("#phone").val()
        }
        $.ajax({
            type: 'POST',
            url: 'login.aspx',
            data: 'newuser=' + JSON.stringify(userdata),
            beforeSend: function () { $('#signuploader').show(); },
            complete: function () { $('#signuploader').hide(); },
            success: function (data) {
                if (data.posted == 'success') {
                    $('.login-form').show();
                    $('.register-form').hide();
                    ResetSignup();
                    $('.login-form .alert-success').show();
                    setTimeout(function () { $('.login-form .alert-success').fadeOut(500); }, 5000);
                } else {
                    $('.login-form .alert-error').show();
                    $('.login-form .alert-error').fadeIn(500);
                }
            }
        });
    }

    var ResetSignup = function () {
        $(".register-form input").val("");
        $(".register-form select").select2("val", "");
        $(".register-form input:checkbox").attr("checked", false).uniform.update();
    }

    var CheckLogin = function () {
        var userpass = {
            username: $("#username").val(),
            password: $("#password").val(),
            returnurl: App.getURLParameter('ReturnUrl')
        }
        $.ajax({
            type: 'POST',
            url: 'login.aspx',
            dataType: 'json',
            data: 'loginuser=' + JSON.stringify(userpass),
            beforeSend: function () { $('#signinloader').show(); },
            complete: function () { $('#signinloader').hide(); },
            success: function (data) {
                if (data.valid) {
                    $('.login-form .alert-success').show().text('You have been successfully logged in');
                    setTimeout(function () {
                        $('.login-form .alert-success').fadeOut(500);
                        var par = App.getFullURLParameter("ReturnUrl");
                        if (par == null) {
                            window.location.href = data.redirect;
                        } else {
                            window.location.href = par;
                        }
                    }, 2000);
                } else {
                    $('.login-form .alert-error').show().find('span').text('That user name and password are invalid. Please try again.');
                    setTimeout(function () { $('.login-form .alert-error').fadeOut(500); }, 5000);
                }
            }
        });
    }

    return {
        //main function to initiate the module
        init: function () { 	
           $('.login-form').validate({
	            errorElement: 'label', //default input error message container
	            errorClass: 'help-inline', // default input error message class
	            focusInvalid: false, // do not focus the last invalid input
	            rules: {
	                username: {
	                    required: true
	                },
	                password: {
	                    required: true
	                },
	                remember: {
	                    required: false
	                }
	            },

	            messages: {
	                username: {
	                    required: "Username is required."
	                },
	                password: {
	                    required: "Password is required."
	                }
	            },

	            invalidHandler: function (event, validator) {
	                $('.alert-error', $('.login-form')).show();
	                setTimeout(function () { $('.alert-error', $('.login-form')).fadeOut(500); }, 3000);
	            },

	            highlight: function (element) { // hightlight error inputs
	                $(element)
	                    .closest('.control-group').addClass('error'); // set error class to the control group
	            },

	            success: function (label) {
	                label.closest('.control-group').removeClass('error');
	                label.remove();
	            },

	            errorPlacement: function (error, element) {
	                error.addClass('help-small no-left-padding').insertAfter(element.closest('.input-icon'));
	            },

	            submitHandler: function (form) {
	                CheckLogin();
	            }
	        });

	        $('.login-form input').keypress(function (e) {
	            if (e.which == 13) {
	                if ($('.login-form').validate().form()) {
	                    CheckLogin();
	                }
	                return false;
	            }
	        });

	        $('.forget-form').validate({
	            errorElement: 'label', //default input error message container
	            errorClass: 'help-inline', // default input error message class
	            focusInvalid: false, // do not focus the last invalid input
	            ignore: "",
	            rules: {
	                email: {
	                    required: true,
	                    email: true
	                }
	            },

	            messages: {
	                email: {
	                    required: "Email is required."
	                }
	            },

	            invalidHandler: function (event, validator) { //display error alert on form submit   

	            },

	            highlight: function (element) { // hightlight error inputs
	                $(element)
	                    .closest('.control-group').addClass('error'); // set error class to the control group
	            },

	            success: function (label) {
	                label.closest('.control-group').removeClass('error');
	                label.remove();
	            },

	            errorPlacement: function (error, element) {
	                error.addClass('help-small no-left-padding').insertAfter(element.closest('.input-icon'));
	            },

	            submitHandler: function (form) {
	                //window.location.href = "index.html";
	            }
	        });

	        $('.forget-form input').keypress(function (e) {
	            if (e.which == 13) {
	                return false;
	            }
	        });

	        jQuery('#forget-password').click(function () {
	            jQuery('.login-form').hide();
	            jQuery('.forget-form').show();
	        });

	        jQuery('#back-btn').click(function () {
	            jQuery('.login-form').show();
	            jQuery('.forget-form').hide();
	        });

	        $('.register-form').validate({
	            errorElement: 'label', //default input error message container
	            errorClass: 'help-inline', // default input error message class
	            focusInvalid: false, // do not focus the last invalid input
	            ignore: "",
	            rules: {
	                selectstore: {
	                    required: true
	                },
	                newusername: {
	                    required: true,
	                    remote: { url: "login.aspx", type: "POST" }
	                },
	                firstname: {
	                    required: true
	                },
	                lastname: {
	                    required: true
	                },
	                password: {
	                    required: true,
                        minlength: 10
	                },
	                rpassword: {
	                    equalTo: "#register_password"
	                },
	                email: {
	                    required: true,
	                    email: true,
	                    remote: { url: "login.aspx", type: "POST" }
	                },
	                phone: {
	                    required: true
	                },
	                tnc: {
	                    required: true
	                }
	            },

	            messages: { // custom messages for radio buttons and checkboxes
	                tnc: {
	                    required: "Please accept TNC first."
	                },
	                newusername: {
                        remote: "That username is already in use. Please use another name."
	                },
	                email: {
                        remote: "That email is already in use. Please try another one."
	                }
	            },

	            invalidHandler: function (event, validator) { //display error alert on form submit   

	            },

	            highlight: function (element) { // hightlight error inputs
	                $(element)
	                    .closest('.control-group').addClass('error'); // set error class to the control group
	            },

	            success: function (label) {
	                label.closest('.control-group').removeClass('error');
	                label.remove();
	            },

	            errorPlacement: function (error, element) {
	                if (element.attr("name") == "tnc") { // insert checkbox errors after the container                  
	                    error.addClass('help-small no-left-padding').insertAfter($('#register_tnc_error'));
	                } else if (element.attr("name") == "selectstore") {
	                    error.addClass('help-small no-left-padding').insertAfter($('#selectstore'));
	                } else {
	                    error.addClass('help-small no-left-padding').insertAfter(element.closest('.input-icon'));
	                }
	            },

	            submitHandler: function (form) {
	                CreateAccount();
	            }
	        });

	        jQuery('#register-btn').click(function () {
	            jQuery('.login-form').hide();
	            jQuery('.register-form').show();
	        });

	        jQuery('#register-back-btn').click(function () {
	            jQuery('.login-form').show();
	            jQuery('.register-form').hide();
	        });

	        $('#selectstore').select2({
	            placeholder: "Select a Store",
	            allowClear: false,
	            width: 'off'
	        });

	        $.backstretch([
		        "assets/img/bg/bucees1.jpg",
                "assets/img/bg/bucees2.jpg",
                "assets/img/bg/bucees3.jpg"
		        ], {
		          fade: 1000,
		          duration: 8000
		      });
        }

    };

}();