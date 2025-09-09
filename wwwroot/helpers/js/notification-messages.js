function notificationMessages(type, msgs) {
	
	if(type==='success'){
		toastr.success(msgs)
	}else if(type==='error'){
		toastr.error(msgs)
	}else if(type==='warning'){
		toastr.warning(msgs)
	}else if(type==='info'){
		toastr.info(msgs)
	}else{
		toastr.info(msgs)
	}
}
