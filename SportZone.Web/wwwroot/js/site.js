// Không cho chọn giờ kết thúc trước giờ bắt đầu trong các form tìm sân / đặt sân
document.querySelectorAll('input[name="gioBatDau"]').forEach(function (bd) {
  var form = bd.closest('form');
  var kt = form && form.querySelector('input[name="gioKetThuc"]');
  if (!kt) return;
  bd.addEventListener('change', function () {
    if (kt.value && kt.value <= bd.value) kt.value = '';
    kt.min = bd.value;
  });
});
