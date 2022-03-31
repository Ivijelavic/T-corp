(function ($) {
    $(document).ready(function () {
		$("table.sortable").tablesorter();
        $("table#devices-list.filterable").addTableFilter({
            labelText: "Pronađi uređaj ",
            size:48
        });
        $("table#tariffs-list.filterable").addTableFilter({
            labelText: "Pronađi tarifu ",
            size: 48
        });
		
		$('#devices-list .device-item').filter(':has(:checkbox:checked)')
            .addClass('selected')
            .end()
          .click(function (event) {
              $(this).toggleClass('success');
              if (event.target.type !== 'checkbox') {
                  $(':checkbox', this).attr('checked', function () {
                      return !this.checked;
                  });
              }
          });

          $('#plans-list .plan-item').filter(':has(:checkbox:checked)')
            .addClass('selected')
            .end()
          .click(function (event) {
              $(this).toggleClass('success');
              if (event.target.type !== 'checkbox') {
                  $(':checkbox', this).attr('checked', function () {
                      return !this.checked;
                  });
              }
          });
	
    });
})($);