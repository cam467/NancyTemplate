var BucApiSettings = (function() {

    var _url = '/api/config/',
        _sel = { oldy: 0 },

        loadScripts = function() {
            // Do work
        },

        _isNumber = function(n) {
            return !isNaN(parseFloat(n)) && isFinite(n);
        },

        _isNumberandNotZero = function(n) {
            if (!isNaN(parseFloat(n)) && isFinite(n)) {
                return !parseFloat(n) == 0;
            } else {
                return !isNaN(parseFloat(n)) && isFinite(n);
            }
        },

        settingsObject = function() {
            var ulfsettings = [];
            $('#settingsform div.controls').each(function(i, e) {
                var ids = '',
                    values = '',
                    c = false;
                $('input:not([type="hidden"]),select', e).each(function(i, e) {
                    if (this.type == 'checkbox' || this.type == 'radio') {
                        if (this.name) {
                            c = true;
                            ids = this.name;
                            if (this.checked) values += this.value + ',';
                        } else {
                            ids = this.id;
                            values = this.checked ? 1 : 0;
                        }
                    } else {
                        ids = this.id;
                        values = this.value;
                    }
                });
                var t = $('table.table-datacolumnlist', e);
                if (t.length) {
                    ids = t.attr('id');
                    values = $(t).find('tbody td:first-child').map(function() {
                        return this.innerText;
                    }).get().join('%3B');
                }
                var et = $('table.table-datatableedit', e);
                if (et.length) {
                    ids = et.attr('id');
                    var headers = $(et).find('thead th');
                    var table = [];
                    $(et).find('tbody tr').each(function(i, e) {
                        var o = {};
                        $(this).find('td').each(function(i) {
                            if (headers[i].innerText.length) o[headers[i].innerText.replace(' ', '_')] = $(this)[0].innerText;
                        });
                        table.push(o);
                    });
                    values = table;
                }
                if (/,$/.test(values) && c) values = values.slice(0, -1);
                ulfsettings.push({
                    name: ids,
                    value: et.length ? JSON.stringify(values) : encodeURIComponent(values)
                });
            });
            return ulfsettings;
        },

        handleSearchitems = function(e) {
            var _this = e;
            $.ajax({
                type: 'GET',
                url: _url + _this.id.replace('search', ''),
                dataType: 'json',
                beforeSend: function() {
                    if ($(_this).hasClass('error')) $(_this).removeClass('error');
                    $(_this).children('i').removeClass('fa-search').addClass('fa-cog fa-spin');
                },
                success: function(data) {
                    //load the data in a modal dialog
                    $(_this).children('i').addClass("fa-search");
                    if (data) {
                        var _b = $('#modalselect').find('div.modal-body');
                        $('#modalselect').find('div.modal-header h3 span').text($(_this).parent().prev().text());
                        var _s = '<ul class="modal-select" data-for="' + _this.id + '">';
                        for (var i = 0; i < data.length; i++) {
                            _s += '<li data-value="' + data[i] + '">' + data[i] + '</i>';
                        }
                        _s += '</ul>';
                        _b.html(_s).end().modal('show');
                    }
                },
                complete: function() {
                    $(_this).children('i').removeClass('fa-cog fa-spin');
                },
                error: function() {
                    $(_this).addClass('error').children('i').addClass('fa-times');
                }
            })
        },

        handleSearchSelect = function(e) {
            var _this = e,
                _value = $(_this).data('value'),
                _for = $(_this).parent().data('for');
            $(_this).parents('div.modal').modal('hide');
            $('#' + _for).prev().val(_value);
        },

        handleFieldMapping = function(ele, e) {
            if (!e.target.value) return;
            var v = +e.target.value;
            var t = $(ele).parents('tbody').find('td:first-child').not(ele).toArray();
            t.sort(function(a, b) {
                return +a.innerText - +b.innerText;
            });
            for (var i = 0; i < t.length; i++) {
                if (+t[i].innerText == v) {
                    t[i].innerText = v + 1;
                    v++;
                }
            }
        },

        saveSettings = function() {
            // console.log(JSON.stringify(settingsObject()));
            $.ajax({
                type: 'POST',
                url: _url + 'settings',
                dataType: 'json',
                contentType: 'application/json',
                data: JSON.stringify(settingsObject()),
                success: function(data) {
                    if (data) {
                        $('button#submitdata').fadeOut('fast', function() {
                            $(this).addClass('success').html('<i class="fa fa-check"></i> Saved').fadeIn();
                        });
                    } else {
                        $('button#submitdata').fadeOut('fast', function() {
                            $(this).addClass('warning').html('<i class="fa fa-exclamation-triangle"></i> Not Saved').fadeIn();
                        });
                    }
                },
                error: function(j, t, e) {
                    console.log(t, e);
                    $('button#submitdata').fadeOut('fast', function() {
                        $(this).addClass('warning').html('<i class="fa fa-exclamation-triangle"></i> Not Saved').fadeIn();
                    });
                }
            });
        },

        getSettings = function(id, title) {
            var st = 0;
            $.ajax({
                type: 'POST',
                url: _url + 'settings/' + id,
                dataType: 'html',
                data: 'title=' + title,
                beforeSend: function() {
                    st = setTimeout(function() {
                        Base.blockUI('div.page-content');
                    }, 2000);
                },
                complete: function() {
                    clearTimeout(st);
                    Base.unblockUI('div.page-content');
                },
                success: function(data) {
                    $('#pagecontenthead').fadeOut('fast', function() {
                        $(this).html(data).fadeIn('fast');
                        Base.initUniform();
                        $('.table-datatable').dataTable({
                            'aaSorting': [
                                [0, 'desc']
                            ]
                        });
                    });
                }
            });
        },

        startUp = function() {
            // Event Binding
            $('.page-sidebar-menu > li:not(:first)').on('click', function(e) {
                $(this).parent().find('li').removeClass('selected').end().end().addClass('selected');
                getSettings($(this).data('section'), $(this).data('title'));
            });
            $(document).on('click', '.page-sidebar-menu a', function(e) {
                // e.preventDefault();
                // e.stopPropagation();
                return false;
            }).on('click', 'button[type="submit"]', function(e) {
                saveSettings();
                return false;
            }).on('selectstart', function(e) {
                return false;
            }).on('click', 'button#trgtrigger', function(e) {
                $.ajax({
                    type: 'GET',
                    url: _url + 'scheduler/trigger',
                    success: function(data) {
                        console.log(data);
                    }
                });
            }).on('click', 'button#logclearlog', function(e) {
                $.ajax({
                    type: 'GET',
                    url: _url + 'log/clear',
                    dataType: 'json',
                    success: function(data) {
                        if (data.success) {
                            $('.table-datatable').dataTable().fnClearTable();
                        }
                    }
                });
            }).on('click', 'button#schrestart', function(e) {
                $.ajax({
                    type: 'GET',
                    url: _url + 'scheduler/restart',
                    dataType: 'json',
                    success: function(data) {

                    }
                });
            }).on('click', 'button[id^="search"]', function(e) {
                handleSearchitems(this);
            }).on('click', 'table.table-datacolumnlist tbody td:first-child, table.table-datatableedit th:not(td:last-child), table.table-datatableedit td:not(td:last-child)', function(e) {
                var _t = this.innerText.trim();
                $(this).css("color", "transparent"), _top = $(this).offset().top, _left = $(this).offset().left;
                if (!$('div.fieldmapinputs').length) $('body').append('<div class="fieldmapinputs"><input type="text" class="fieldmapinput"/><div class="editbuttons" style="display:none;"><i class="fa fa-plus"></i><i class="fa fa-minus"></i></div></div>');
                $('div.fieldmapinputs').css({
                    top: _top,
                    left: _left
                }).show();
                $('input.fieldmapinput').val(_t).data('ele', this).show().focus();
                $('div.fieldmapinputs div.editbuttons').css("display", $(this).is('th') ? "block" : "none");
            }).on('click', 'div.editbuttons i.fa-minus, div.editbuttons i.fa-plus', function(e) {
                $('div.fieldmapinputs').hide();
                var pe = $(this).closest('tr').length ? 'tr' : 'th',
                    el = pe == 'th' ? $(this).closest('div.fieldmapinputs').find('input').data('ele') : this,
                    isdelete = $(this).hasClass('fa-minus');
                switch (pe) {
                    case 'tr':
                        if (isdelete) {
                            $(el).closest(pe).remove();
                        } else {
                            $(el).closest(pe).clone().insertAfter($(el).closest(pe));
                        }
                        break;
                    case 'th':
                        if (isdelete) {
                            $(el).closest('table').find('tbody tr').each(function(i, e) {
                                $(e).find('td:nth-child(' + ($(el).index() + 1) + ')').remove();
                            });
                            $(el).remove();
                        } else {
                            $(el).clone().insertAfter(el);
                            $(el).closest('table').find('tbody tr').each(function(i, e) {
                                $(e).find('td:nth-child(' + ($(el).index() + 1) + ')').clone().insertAfter($(e).find('td:nth-child(' + ($(el).index() + 1) + ')'));
                            });
                        }
                        break;
                }
            }).on('click', 'div.editbuttons i.fa-play-circle', function(e) {
                var nameord = $(this).closest('table').find('th:contains("Name")').index(),
                    jobname = $(this).closest('tr').find('td:nth(' + nameord + ')')[0].innerText;
                $.ajax({
                    type: 'GET',
                    url: _url + 'scheduler/trigger/' + encodeURI(jobname),
                    dataType: 'json',
                    success: function(data) {}
                });
            }).on('click', 'div.configbuttons i.fa-minus, div.configbuttons i.fa-plus, div.configbuttons i.fa-pencil', function() {
                var isdelete = $(this).hasClass('fa-minus'),
                    isedit = $(this).hasClass('fa-pencil'),
                    control = $(this).closest('div.row-fluid'),
                    sectionid = 0,
                    controlkey = $(this).data('controlkey'),
                    prevglokey = '';
                if (isdelete) {
                    $.ajax({
                        method: 'DELETE',
                        url: _url + "settings/" + controlkey,
                        success: function(d) {
                            control.remove();
                        },
                        error: function(d) {
                            console.log(d);
                        }
                    });
                } else {
                    sectionid = $(this).data('sectionid');
                    //this gets the add settings form in html to put in modal
                    $.ajax({
                        method: 'GET',
                        url: _url + "settings/" + (isedit ? "editsetting/" + controlkey : "addsetting"),
                        success: function(d) {
                            $('#modaladdsetting').find('div.modal-header span').html(isedit ? "Edit Setting" : "Add Setting").end().find('div.modal-body').html(d).end().find('#savesetting').off().on('click', function() {
                                var ulfsettings = [];
                                $('#addsettingsform div.controls').each(function(i, e) {
                                    var ids = '',
                                        values = '',
                                        c = false;
                                    $('input:not([type="hidden"]),select', e).each(function(i, e) {
                                        if (this.type == 'checkbox' || this.type == 'radio') {
                                            if (this.name) {
                                                c = true;
                                                ids = this.name;
                                                if (this.checked) values += this.value + ',';
                                            } else {
                                                ids = this.id;
                                                values = this.checked ? 1 : 0;
                                            }
                                        } else {
                                            ids = this.id;
                                            values = this.value;
                                        }
                                    });
                                    ulfsettings.push({
                                        name: ids,
                                        value: encodeURIComponent(values),
                                        previousid: prevglokey
                                    });
                                });
                                // console.log(ulfsettings);
                                $.ajax({
                                    method: isedit ? 'PUT' : 'POST',
                                    url: _url + 'settings/' + (isedit ? 'editsetting' : 'addsetting'),
                                    dataType: 'json',
                                    contentType: 'application/json',
                                    data: JSON.stringify(ulfsettings),
                                    success: function(d) {
                                        console.log(d);
                                    },
                                    error: function(d) {
                                        console.log(d);
                                    }
                                });
                            }).end().modal();
                            if (!isedit) {
                                $('#modaladdsetting div.modal-body #glosection').val(sectionid);
                            } else {
                                prevglokey = $('#modaladdsetting div.modal-body #glokey').val();
                            }
                        },
                        error: function(d) {

                        }
                    });
                }
            }).on('focusout', 'div.fieldmapinputs', function(e) {
                var $this = $(this).find('input');
                $this.data('ele').innerText = $this.val();
                $($this.data('ele')).css('color', '');
                handleFieldMapping($this.data('ele'), e);
                $this.val('');
                if (!$('i.fa:hover').length) $(this).hide();
                // $this.data('ele').innerText = $this.value;
                // this.value = '';
                // $(this).parent().hide();
            }).on('click', 'ul.modal-select li', function(e) {
                handleSearchSelect(this);
            }).on('keypress', 'input.fieldmapinput', function(e) {
                if (e.which == 13) {
                    var $this = $(this);
                    $this.data('ele').innerText = this.value;
                    $($this.data('ele')).css('color', '');
                    $this.hide();
                    $($this.data('ele')).parent().next().find('td:first').trigger('click');
                    return false;
                } else if (!/./.test(String.fromCharCode(e.which))) {
                    return false;
                }
                //this section is for table row reordering
            }).on('mousedown', '.table-datatableedit div.editbuttons i.fa-arrows-v', function(e) {
                e.preventDefault();
                _sel.tablelength = $('.table-datatableedit tbody tr').length;
                if (_sel.tablelength !== 1) {
                    var _row = $(this).closest('tr').index();
                    _sel.element = $('.table-datatableedit tbody tr:nth(' + _row + ')');
                    _sel.element.addClass('dragging');
                }
            }).on('mouseup', function(e) {
                if (_sel.tablelength !== 1) {
                    if (_sel.element) {
                        _sel.element.removeClass('dragging');
                        _sel.element = null;
                    }
                }
            }).on('mousemove', function(e) {
                if (_sel.tablelength !== 1 && _sel.element != null) {
                    if ($('.table-datatableedit').has(e.target).length > 0) {
                        var _el = $(e.target).closest('tr');
                        if (_el.find('th').length == 0) {
                            if (e.pageY > _sel.oldy) {
                                _el.after(_sel.element);
                            } else {
                                _el.before(_sel.element);
                            }
                        }
                    }
                    _sel.oldy = e.pageY;
                }
            });
        };

    return {
        init: startUp,
        settings: settingsObject
    };
})();