/*
 *  Document   : tablesDatatables.js
 *  Author     : pixelcave
 *  Description: Custom javascript code used in Tables Datatables page
 */

var GetProducts = function () {

    return {
        init: function () {
            var loader = $('#loader');
            loader.css('display', 'none');
            /* Initialize Bootstrap Datatables Integration */
            $('#products-container').css('display', 'none');
            $('#amount').attr('readonly', 'readonly');
            //   var form = document.getElementById("template-contactform");

            //form.addEventListener("submit", function (evt) {
            //    var input1 = $("#phone");
            //    var amount = $("#amount");
            //    if (input1.val() == "" || amount.val() == "") {
            //        evt.preventDefault();
            //        return false;
            //       }
            //       var operator = $('#Operators').val();
            //       if (operator == "" || operator == undefined || operator == null) {
            //           evt.preventDefault();
            //           return false;
            //       }

            //       form.submit();

            //   }, false);

            $('#template-contactform').submit(function (evt) {
                var input1 = $("#phone");
                var amount = $("#amount");
                if (input1.val() == "" || amount.val() == "") {
                    evt.preventDefault();
                    return false;
                }
                var operator = $('#Operators').val();
                if (operator == "" || operator == undefined || operator == null) {
                    evt.preventDefault();
                    return false;
                }

                form.submit();
            });


            $('#plans_btn').click(function () {
                var operator = $('#Operators').val();
                //if (operator=="" || operator == undefined || operator == null) {
                //    alert("nul");
                //} else {
                //    alert(operator);
                //}

                $('.bs-example-modal-lg').addClass("fade");
                $('.bs-example-modal-lg').addClass("in");
                $('.bs-example-modal-lg').css("display", "block");
            });
            var selectedProjectId = -1;
            $('#Operators').change(function () {

                var loader = $('#loader');
                var form = $('#form');
              

                var id = $(this).val();

                if (id == "" || id == undefined) {
                    selectedProjectId = -1;
                    $('#products-container').css('display', 'none');
                    $('#snav-content1').css('display', 'block');

                    return;
                }



                if (id != selectedProjectId) {

                    loader.css('display', 'block');
                    form.css('display', 'none');

                    selectedProjectId = id;

                    $.get("/Home/GetProducts", { id: id }, function (data) {

                        loader.css('display', 'none');
                        form.css('display', 'block');


                        $('#products').empty();
                        $('#products-container').css('display', 'block');
                        $('#snav-content1').css('display', 'none');
                        if (data.length == 0) {
                            $('#products').append("No Product");
                            return;
                        }

                       
                        $.each(data, function (i, item) {

                            var ProjectID = item.ProductId;

                            var ProjectName = item.Product_Name;
                            var Product_Price = item.Product_Price;
                            var source = $("#assign-template").html();
                            var template = Handlebars.compile(source);
                            var context = { prjid: ProjectID, prdDesc: ProjectName, price: Product_Price };
                            var html = template(context);

                            $('#products').append(html);




                        });

                        return false;

                    });

                }

            });

            var arr = jQuery.makeArray();

            $("#products").on("click", ".selectProduct", function () {
                var productid = $(this).data('pdid');
                var price = $(this).data('price');
                $('#amount').val(price);
                $('#ProductId').val(productid);
            });





        }
    };
}();
