
//Đây là phần công việc đã thực hiện, được để lại để sau này sửa lại trong file báo cáo, không cần đọc từ dòng này trở đi.


Trang bảo hành của admin và nhân viên sẽ có nút: Tiếp nhận mới, Đang xử lý, Đã xử lý, Hoàn tất & trả khách, Hủy phiếu
Bảo hành có 3 trạng thái: Tiếp nhận, Đang xử lý, Đã xử lý, Đã trả khách.
Nhân viên bán hàng sẽ là người tạo phiếu bảo hành, có thể ghi chi phí dự tính hoặc để trống cũng không sao, có phần để ghi tình trạng sản phẩm khi tiếp nhận, và phần ghi chú. Phiếu bảo hành vừa tạo sẽ có trạng thái là: Tiếp nhận.
Thêm nhân viên kỹ thuật, nhân viên kỹ thuật chỉ có thể thay đổi trạng thái của dịch vụ bảo hành từ tiếp nhận(trạng thái ban đầu khi phiếu bảo hành được khởi tạo bởi nhân viên bán hàng) -> Đang xử lý (để báo rằng sản phẩm đó đang được bảo hành) và từ Đang xử lý -> Đã xử lý (Báo rằng đã sửa xong), ghi nhận chi phí thực tế khi sửa chữa.

Nhân viên bán hàng khi trả lại sản phẩm cho khách sẽ chuyển trạng thái Đã xử lý -> Đã trả khách bằng việc nhấn Hoàn tất và trả khách. Nếu chi phi thực tế khác 0, thì sẽ hiện lên giao diện tính tiền bằng tiền mặt hoặc chuyển khoản giống trang bán offline, sau đó nhấn xác nhận thanh toán mới chuyển trạng thái của phiếu bảo hành. 



trang bán offline phải có ràng buộc về số lượng sản phẩm, sẽ kiểm tra số lượng sản phẩm khi nhân viên thực hiện thao tác thay đổi số lượng, và chỉ bán được những sản phẩm cho phép bán (tức là sản phẩm không bị ẩn khỏi trang mua sắm của người dùng), rà soát lại: có phải đối với khách vãng lai có để lại sdt cho đơn hàng, khi khách hàng tạo tài khoản với số điện thoại đó, hệ thống cũng sẽ tự liên kết lại các đơn hàng đó vô tài khoản vừa tạo.

--------------------------



mỗi sản phẩm nên có nhiều size, hiện tại size được nhập theo kiểu string, cùng một loại sản phẩm nhưng khác size sẽ là sản phẩm khác nhau, về cơ bản là chưa có cơ chế góm nhóm cho nó giả sử sau này khi chọn 1 sản phẩm nó sẽ hiển thị ra nhiều tùy chọn size. Tôi đề xuất thực hiện điều này bằng cách có phần mã nhóm cho sản phẩm, nghĩa là tạo thêm một bảng csdl về mã nhóm, và có tab quản lý nhóm, quy ước các sản phẩm cùng nhóm là các sản phẩm đó phải cùng tên nhưng khác size. Nghĩa là trong csdl có bảng nhóm sản phẩm, sanpham sẽ phải bổ sung thêm phần mã nhóm.
Vậy tôi cần một tab quản lý nhóm sản phẩm trong trang quản lý sản phẩm, trang này có nút thêm nhóm, sẽ hiển thị ra modal cho phép nhập tên của nhóm. Nhưng có thêm phần chọn tên nhóm từ sản phẩm có sẵn với ô search và combobox được hiện ra tương ứng. Điều này không đồng nghĩa với việc sản phẩm được đề xuất tên sẽ được thêm vào nhóm đó. để xác nhận việc chọn tên của sản phẩm này cho tên nhóm, thì khi người dùng search, nó sẽ tìm kiếm và hiển thị ra combobox, tên đầu tiên được đề xuất trong combobox sẽ thay đổi ô tên nhóm. phần cuối của modal sẽ là nút thêm hoặc nút hủy. 
Nút chỉnh sửa nhóm sẽ hiện thị ra modal thay đổi tên nhóm, thay đổi tên nhóm sẽ thay đổi tất cả sản phẩm trong nhóm. Và nó sẽ có phần để hiện danh sách sản phẩm trong nhóm. Nếu danh sách quá dài cỡ trên 4 sản phẩm thì cho phép scroll, danh sách sẽ hiện mã sản phẩm và size vì tên sản phẩm đã trùng với tên nhóm nên không cần hiện, dưới danh sách đó là nút loại khỏi nhóm, nút đó sẽ sáng lên và cho phép bấm khi người dùng chọn 1 sản phẩm nào đó trong danh sách, phía dưới nút đó là ô search và combobox hiển thị tên sản phẩm tìm kiếm được nếu người dùng có ý định thêm sản phẩm, khi người dùng nhập vào ô search, nếu tìm thấy được danh sách sản phẩm nào đó thì nó sẽ hiển thị ở đầu combobox đồng thời phía dưới là label Size nó cũng sẽ bị thay đổi để hiện size của sản phẩm đó Size: {size sản phẩm}, danh sách hiện ra nhớ loại trừ các sản phẩm đã có trong nhóm hiện tại, có thể hiện sản phẩm của nhóm khác, nhưng khi nhấn thêm sẽ cảnh báo: sản phẩm này thuộc về nhóm nào đó, vui lòng hủy nhóm của sản phẩm trước khi thêm, khi người dùng nhấn ok thì nó sẽ làm trống ô search và combobox. việc thêm sản phẩm vào nhóm xảy ra khi người dùng nhấn vào nút thêm phía dưới ô search và combobox đó. Giả sử sản phẩm chuẩn bị thêm vào nhóm có chuỗi string size bị trùng với sản phẩm nào đó trong nhóm thì hiện thông báo và không cho thêm. Phía cuối modal là nút lưu thay đổi, hủy, xóa nhóm, khi nhấn xóa nhóm phải hỏi xác nhận. Khi nhóm bị xóa, các sản phẩm trong nhóm đó sẽ được đặt mã nhóm thành 0.
Các sản phẩm mới được tạo sẽ có một mã nhóm mặc định được cho là 0, mã nhóm mặc định này nghĩa là các sản phẩm không gom nhóm, khi loại 1 sản phẩm khỏi nhóm thì nó sẽ đặt mã nhóm của sản phẩm đó về lại mã mặc định.  
Vì thế khi thêm một sản phẩm không cùng tên vào một nhóm nào đó, khi người dùng nhấn xác nhận nó sẽ hỏi: Thêm sản phẩm này vào nhóm sẽ đổi tên sản phẩm này thành: {name}. {name} này là tên của sản phẩm được thêm vào nhóm trước đó. Khi một sản phẩm thuộc về một nhóm, khi người dùng đổi tên sản phẩm bằng chức năng chỉnh sửa chi tiết sản phẩm, nó sẽ hỏi xác nhận: Thay đổi tên sản phẩm sẽ đổi tên các sản phẩm khác trong cùng nhóm và tên nhóm(nút lưu sản phẩm. Nếu người dùng ok thì nó sẽ thực hiện, nếu không thì nó sẽ ngừng quá trình thay đổi này, đặt tên sản phẩm về lại tên trùng với tên của nhóm, nhấn lưu sản phẩm là thay đổi được). 

tại nút sửa chi tiết sản phẩm, khi người dùng chỉnh sửa size, vì là chuỗi string, nên nếu sản phẩm đang thuộc 1 nhóm nào đó, nó sẽ kiểm tra xem size vừa chỉnh có bị trùng với sản phẩm nào trong nhóm đó không, nếu trùng rồi thì khi người dùng nhấn lưu thay đổi nó sẽ phát ra cảnh báo: size của sản phẩm đã bị trùng với sản phẩm khác trong nhóm, khi người dùng nhấn ok, nó sẽ đưa nội dung trong ô size về nội dung ban đầu (cơ chế kiểm tra sẽ thực thi khi người dùng nhấn lưu thay đổi). chỉnh sửa chi tiết nếu sản phẩm đã có nhóm thì combobox sẽ hiển thị mã nhóm đó. Tức là thêm sản phẩm và sửa sản phẩm không cần cho phép tùy chỉnh nhóm. Nhưng sửa sản phẩm nếu có nhóm thì hiện mã nhóm, không có thì ghi chưa thuộc về nhóm nào Mã nhóm: {mã nhóm}.

 Trong trang Quản lý sản phẩm, phần hiển thị danh sáhc sản phẩm cũng sẽ có cột hiện thêm phần mã nhóm kế bên cột tên trang sức


Nhớ là đảm bảo các nội dung được sửa sẽ có đủ khoảng cách hiển thị, không bị che. khi nội dung quá dài vượt quá chiều ngang thì cứ thêm thanh scroll ngang là được. Các tính năng mới được thêm vào phần hiển thị cũng phải đảm bảo sẽ không bị che, chồng lên nhau.



sau khi đã thực hiện được yêu cầu trên, hãy vào phần nhập kho sản phẩm, tôi muốn modal phiếu nhập kho có phần search và combobox chọn sản phẩm thay vì chỉ có comboxbox chọn sản phẩm như hiện tại. Khi người dùng search, nó sẽ hiện sản phẩm tương ứng trong combobox, nội dung trong đó gồm mã sản phẩm và tên sản phẩm, nếu sản phẩm thuộc về một nhóm nào đó, nó sẽ có nhiều size, nên hãy thêm combobox cho chọn size sẽ cho phép đổi size bằng combobox. nếu sản phẩm không có nhóm nó chỉ có 1 size thì combobox hiển thị 1 size đó là được. Giả sử sản phẩm có nhiều size thì khi chọn size nhớ thay đổi lại sản phẩm đang được chọn trong combobox tương ứng với mã sản phẩm của size đó. Phiếu nhập kho vừa mở lên đang có dữ liệu mặc định, xử lý giúp tôi, thêm cho tôi nút xóa kế bên bảng sản phẩm được nhập cho từng sản phẩm được nhập lỡ người dùng chọn nhầm sản phẩm cần nhập.


sau đó vào trang quản lý bán hàng tại quầy pos, phần thêm sản phẩm chỗ chọn sản phẩm hiển thị mã sản phẩm với tên sản phẩm là được, phía dưới là combobox cho chọn size. cơ chế tương tự chọn size của nhập kho.

----------------------------
Tôi muốn sửa một chút tại phần quản lý kho hàng, thay vì các sản phẩm chi tiết của phiếu nhập hiện trong bảng phía dưới, tôi muốn bỏ bảng đó đi, có một nút xem chi tiết bên phải nút Lập phiếu nhập kho, nhân vào đó sẽ hiện ra chi tiết các thứ có trong phiếu nhập đó. 

đối với tài khoản admin, sẽ có một nút dùng để vô hiệu hóa phiếu nhập trong trường hợp nhân viên lập ra một phiếu nhập lỗi, vậy cần thêm 1 thuộc tính trong csdl cho phiếu nhập dùng để xem coi đó có phải phiếu nhập hợp lệ không. Nếu quản trị viên chọn vô hiệu hóa thì phiếu đó sẽ được đánh dấu là vô hiệu hóa. Tồn kho sẽ thay đổi tương ứng, giá bán bình quân cũng được tính lại theo ngữ cảnh phiếu nhập đó chưa từng tồn tại. Tuyệt đối không cho sửa phiếu nhập. 

Tôi muốn thêm 1 tab là Kiểm kê, trong tab này đối với nhân viên kho sẽ lập được phiếu kiểm kê, phiếu kiểm kê đó sẽ do quản lý duyệt, quản lý duyệt ok thì giả sử tồn kho lệch, quản lý duyệt xong thì tồn kho thực tế mới bị trừ. Nhớ là trong phiếu kiểm kê có thể kiểm kê cùng lúc nhiều sản phẩm, dùng cơ chế search và chọn bằng combobox để chọn sản phẩm muốn lập kiểm kê khi có chênh lệch. Khi sản phẩm đầu tiên được tìm thấy hiện trong combobox hoặc người dùng mở combobox và chọn được sản phẩm thì phía dưới là một textbox chỉ đọc hiện số lượng tồn kho trong hệ thống, bên phải textbox đó là một textbox khác cho nhập tồn kho thực tế. Sau đó nhấn thêm thì sản phẩm cùng với số lượng nằm trong hệ thống, và số lượng thực tế mới hiện trong bảng phía dưới đồng thời cũng làm trống search (mặc định combobox khi ô search trống sẽ hiển thị tất cả sản phẩm). tất nhiên là dưới bảng chứa danh sách sản phẩm kiểm kê là nút dùng để loại sản phẩm đó khỏi danh sách kiểm kê giả sử nhân viên nhập nhầm, phía dưới nút loại đó là chỗ cho nhập ghi chú. Phía cuối cùng modal sẽ là nút Thêm biên bản kiểm kê và nút hủy.
Khi thêm xong, phiếu kiểm kê đó sẽ nằm trong danh sách các phiếu kiểm kê, sẽ hiện theo thứ tự các phiếu mới nhất, phiếu đó sẽ có trạng thái chờ duyệt, tất nhiên cũng có nút xem chi tiết phiếu kiểm kê cạnh nút thêm, nút này sẽ mở lại modal chi tiết của phiếu kiểm kê, phía cuối modal đối với nhân viên kho sẽ có 2 nút là Đóng và nút Lưu thay đổi, cho phép sửa lại phiếu đó nếu phiếu đó chưa được duyệt. Admin khi mở chi tiết phiếu đó lên, thì ngoài 2 nút đó sẽ có thêm nút duyệt phiếu, duyệt sẽ thay đổi trạng thái của phiếu đó, đóng modal, thay đổi trạng thái phiếu trong danh sách hiển thị. Khi xem chi tiết phiếu đó thì chỉ còn nút đóng phiếu chứ không có nút lưu thay đổi nữa. Cũng không cho sửa bất kì chỗ nào. Phiếu đã được duyệt thì tồn kho sản phẩm sẽ thay đổi, chi thay đổi tồn kho thôi là được không cần thay đổi thêm gì khác. Khi mở chi tiết một phiếu đã được duyệt, đối với nhân viên thì cuối modal chỉ có nút đóng, đối với admin sẽ có thêm nút Vô hiệu hóa dùng để vô hiệu hóa phiếu, trả lại số lượng tồn kho ban đầu nếu duyệt nhầm phiếu. Có thêm một nút xóa kế nút xem chi tiết phiếu. Nút này chỉ sáng đối với các phiếu chưa được duyệt, nhân viên và admin đều dùng được.

Tôi muốn bạn lưu ý giả sử có phiếu kiểm kê chồng chéo lên nhau. Thì tôi muốn khi phiếu bị vô hiệu hóa phải có cơ chế trả lại số sản phẩm tồn kho một cách chính xác chứ không phải lấy số lượng được ghi trong phiếu và sửa lại. Ví dụ lỡ như khi duyệt phiếu có sản phẩm thực tế còn 100 cái nhưng hệ thống ghi 105 cái, sau đó khách mua 2 cái thì còn 98 cái, sau đó vô hiệu hóa phiếu, thì không thể nào trả lại số sản phẩm trong tồn kho là 105 được, nó sẽ là 103 cái. Vấn đề ràng buộc, giả sử có 2 phiếu cùng sửa 1 sản phẩm được gửi đến cho admin duyệt giả khi duyệt 1 phiếu, thì số lượng tồn kho thực tế của sản phẩm đã khác so với số lượng tồn kho thực tế trong hệ thống ghi nhận của phiếu còn lại. Lúc này bạn phải kiểm tra và từ chối cho duyệt phiếu đó với lí do là số lượng tồn kho được ghi nhận của phiếu không chính xác. Phiếu kiểm kê không chỉ ghi nhận thâm hụt, mà còn có thể ghi nhận nếu số lượng thực tế nhiều hơn so với số lượng tồn tại trong hệ thống. Giả sử phiếu kiểm kê 1 vào ngày hôm qua ghi nhận 1 sản phẩm đã được duyệt, phiếu kiểm kê 2 vào ngày hôm nay ghi nhận cùng sản phẩm đó và đã được duyệt. Lúc này nếu admin vô hiệu hóa phiếu kiểm kê 1 sẽ không cho. Nó sẽ thông báo chỉ cho vô hiệu hóa phiếu có chỉnh sửa mới nhất đối với sản phẩm. Lúc này admin có 2 lựa chọn là lập phiếu kiểm kê khác để bù lại số lượng. Hoặc là vô hiệu hóa phiếu kiểm kê kia trước.


Tôi muốn phần nhập phiếu bảo hành nó cũng sẽ theo cơ chế ô search và combobox, tôi muốn xóa bỏ phần thông tin nhập sẵn trong modal tạo phiếu bảo hành đi, và cũng phải cho khách vãng lai không có tài khoản bảo hành, nhớ ghi nhận lại sdt của khách. Sau này khách tạo tài khoản thì ngoài đơn hàng được liên kết, phiếu bảo hành cũng được liên kết đến.


// Đã hoàn tất toàn bộ các yêu cầu gần nhất (Sửa lỗi tạo phiếu bảo hành, Sửa tên nhóm đồng bộ SP, Hiển thị web theo nhóm).



// Đã hoàn tất:
// 1. Tùy chỉnh tên nhóm độc lập, bỏ việc đổi tên nhóm/các SP khác khi sửa 1 sản phẩm trong nhóm, hiển thị tên nhóm trên Web.
// 2. ChiTietHoaDon ghi nhận % giảm giá tại thời điểm mua, đơn giá lưu giá gốc chưa giảm; chuẩn hóa HoaDon.TinhTongTien() và luồng bán Offline POS & Online Web.


Phiếu nhập bây giờ tài khoản nhân viên kho chỉ có thể tạo phiếu nhập xong chờ admin duyệt phiếu thì mới tính vào tồn kho, thay đổi giá nhập bình quân. Để hủy một phiếu nhập đã được admin duyệt, phiếu nhập đó phải đáp ứng điều kiện, khi tồn kho trừ đi số lượng sản phẩm của phiếu nhập đó, tồn kho của sản phẩm bất kỳ trong phiếu nhập đó sau khi trừ không được âm.
Thêm nút xóa phiếu tồn kho, điều kiện là phiếu đó chưa được admin phê duyệt, cả nhân viên và admin đều xài được.[yêu cầu mới]
Bỏ đi tính năng chỉnh sửa phiếu kiểm kê chưa được phê duyệt đi, phiếu kiểm kê nếu lỗi thì xóa tạo lại chứ không cho sửa trên đó.[yêu cầu mới]
Thay đổi cơ chế giá bán, tại trang sản phẩm, chi tiết sẩn phẩm, tùy chỉnh giá bằng %lợi nhuận thay vì nhập tay giá bán, mặc định khi tạo sản phẩm % lợi nhuận là 0%, ô giá bán sẽ bị khóa không cho nhập. giá bán sẽ được tính lại khi thay đổi tỉ lệ lợi nhuận, hoặc có phiếu nhập kho cho sản phẩm đó làm thay đổi giá nhập bình quân của sản phẩm, hoặc khi phiếu nhập kho bị vô hiệu hóa. Giá bán được tính theo công thức: Giá bán = giá nhập bình quân * (1 + tỷ lệ lợi nhuận). Vậy SanPhamTrangSuc cần bổ sung thêm TyLeLoiNhuan trong csdl.


----------------

triển khai đồ án này đọc csdl từ docker.

Tính năng thêm sản phẩm mặc định sản phẩm mới tạo trong form sẽ để là chưa phân loại giống với phân loại của sản phẩm nếu danh mục đó bị xóa. Tôi nghĩ là Danh mục chưa phân loại nên hiển thị trong trang danh mục sản phẩm của app winform. Trong trang quản lý sản phẩm trang sức cũng lọc được sản phẩm chưa có danh mục. Nghĩa là tôi muốn có 1 danh mục tên là Chưa phân loại thay vì nó là một dạng ngầm định không được hiển thị nếu một danh mục bị xóa và các sản phẩm thuộc danh mục đó được ngầm định là chưa phân loại.

Quản lý nhập kho hàng, tạo phiếu nhập kho đang bị gán sẵn một mã nhân viên tạo phiếu thay vì lấy mã của người tạo phiếu đó. Khi dùng tính năng tìm kiếm tên sản phẩm, nó hiện ra sản phẩm ứng với tên đó, nhưng mà nếu sản phẩm đó thuộc về nhóm, thì size vẫn hiển tất cả các size của nhóm đó, nhưng nhấn đổi không có tác dụng do ô tìm kiếm đang chứa ký tự, tôi muốn khi bấm đổi size thì nó làm trống ô đó để tránh lỗi này. Tính năng vô hiệu hóa phiếu nhập kho có trừ đi tồn kho nhưng không khôi phục lại hiện trạng giá bán với giá nhập bình quân.

Tạo đơn bán hàng offline, SP001 có tồn kho là 2 nhưng không hiển thị để chọn lúc tạo hóa đơn.

Tạo phiếu bỏa hành tôi muốn bỏ KH_GUEST_POS đi, khách vãng lai nếu không để lại số điện thoại thì thôi cũng không sao. Khi nhập số điện thoại, khi nhập xong hệ thống sẽ tự kiểm tra xem người dùng này có trong hệ thống hay không và liên kết đến.


Tại trang quản lý sản phẩm thêm nút Ẩn/Hiển sản phẩm, vậy thêm một cột để biết xem sản phẩm đó có bị ẩn không. Nếu sản phẩm bị ẩn thì nó sẽ không xuất hiện ở web mua hàng của user và tại tạo hóa đơn bán hàng offline sẽ không hiển thị sản phẩm đó để chọn