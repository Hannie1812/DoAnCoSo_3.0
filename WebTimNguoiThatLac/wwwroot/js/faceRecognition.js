// Biến toàn cục
let faceMatcher;
let abortController = null;

// Khởi tạo face-api.js
async function initFaceRecognition() {
  try {
    showLoading(true, 0, "Đang tải mô hình nhận diện...");

    // Đảm bảo đường dẫn đúng
    const modelPath = "/models"; // hoặc './models' tùy vào cấu trúc project

    // Tải model với error handling rõ ràng
    await Promise.all([
      //faceapi.nets.ssdMobilenetv1.load(modelPath).catch(e => {
      //    console.error('Lỗi tải ssdMobilenetv1:', e);
      //    alert(`Lỗi tải ssdMobilenetv1`);
      //    throw e;
      //}),
      //faceapi.nets.faceLandmark68Net.load(modelPath).catch(e => {
      //    console.error('Lỗi tải faceLandmark68Net:', e);
      //    alert(`Lỗi tải faceLandmark68Net`);
      //    throw e;
      //}),
      //faceapi.nets.faceRecognitionNet.load(modelPath).catch(e => {
      //    console.error('Lỗi tải faceRecognitionNet:', e);
      //    alert(`Lỗi tải faceRecognitionNet`);
      //    throw e;
      //})
      /* faceapi.nets.ssdMobilenetv1.loadFromUri("/models"),
      faceapi.nets.faceLandmark68Net.loadFromUri("/models"),
      faceapi.nets.faceRecognitionNet.loadFromUri("/models"), */
      faceapi.nets.tinyFaceDetector.loadFromUri(
        "https://cdn.jsdelivr.net/gh/justadudewhohacks/face-api.js@master/weights"
      ),
      faceapi.nets.faceLandmark68Net.loadFromUri(
        "https://cdn.jsdelivr.net/gh/justadudewhohacks/face-api.js@master/weights"
      ),
      faceapi.nets.faceRecognitionNet.loadFromUri(
        "https://cdn.jsdelivr.net/gh/justadudewhohacks/face-api.js@master/weights"
      ),
    ]);

    console.log("Các model nhận diện đã sẵn sàng");
    showLoading(false);
    return true;
  } catch (error) {
    console.error("Lỗi khi tải mô hình:", error);
    showLoading(false);

    // Hiển thị thông báo chi tiết hơn
    const specificError = error.message.includes("Failed to fetch")
      ? "Không thể tải file model. Kiểm tra đường dẫn và CORS."
      : error.message;

    alert(`Không thể tải mô hình nhận diện: ${specificError}`);
    return false;
  }
}

// Hiển thị tiến trình
function showLoading(show, progress = 0, message = "") {
  const loadingIndicator = document.getElementById("loadingIndicator");
  const progressBar = document.getElementById("progressBar");
  const progressText = document.getElementById("progressText");
  const progressMessage = document.getElementById("progressMessage");

  if (loadingIndicator) {
    loadingIndicator.style.display = show ? "block" : "none";
  }

  if (progressBar) {
    progressBar.style.width = `${progress}%`;
    progressBar.setAttribute("aria-valuenow", progress);
  }

  if (progressText) {
    progressText.innerText = `${progress}%`;
  }

  if (progressMessage) {
    progressMessage.innerText = message || `Đang xử lý... (${progress}%)`;
  }
}

// Hủy bỏ tìm kiếm
function cancelSearch() {
  if (abortController) {
    abortController.abort();
    showLoading(false);
    console.log("Đã hủy tìm kiếm");
  }
}

// Xử lý upload ảnh
async function handleImageUpload(event) {
  const file = event.target.files[0];
  if (!file) return;

  // Tạo AbortController để có thể hủy request
  abortController = new AbortController();

  try {
    // Reset kết quả cũ
    document.getElementById("searchResults").innerHTML = "";

    // Kiểm tra kích thước ảnh
    if (file.size > 5 * 1024 * 1024) {
      // 5MB
      throw new Error("Ảnh quá lớn. Vui lòng chọn ảnh nhỏ hơn 5MB");
    }

    showLoading(true, 5, "Đang gửi ảnh lên server...");

    // Gửi ảnh lên server
    const formData = new FormData();
    formData.append("file", file);

    const response = await fetch("/TimNguoi/TimKiemBangHinhAnh", {
      method: "POST",
      body: formData,
      signal: abortController.signal,
    });

    if (!response.ok) {
      throw new Error(`Lỗi server: ${response.status}`);
    }

    const data = await response.json();
    showLoading(true, 10, "Đang phân tích ảnh...");

    // Xử lý nhận diện
    const results = await processFaceRecognition(data.tempImageUrl, data.posts);

    // Hiển thị kết quả
    showLoading(true, 100, "Hoàn thành!");
    setTimeout(() => {
      displayResults(results);
      showLoading(false);
    }, 800);
  } catch (error) {
    if (error.name === "AbortError") {
      console.log("Request đã bị hủy");
      return;
    }

    console.error("Lỗi khi tìm kiếm:", error);
    showLoading(false);

    // Hiển thị thông báo lỗi chi tiết
    const errorMessage = error.message.includes("server")
      ? "Lỗi kết nối đến server"
      : error.message;

    document.getElementById("searchResults").innerHTML = `
            <div class="alert alert-danger">
                <strong>Lỗi:</strong> ${errorMessage}
                ${
                  error.message.includes("khuôn mặt")
                    ? "<p>Vui lòng chọn ảnh có khuôn mặt rõ ràng</p>"
                    : ""
                }
            </div>
        `;
  } finally {
    abortController = null;
  }
}

// Xử lý nhận diện khuôn mặt
async function processFaceRecognition(tempImageUrl, posts) {
  showLoading(true, 15, "Đang tải ảnh...");
  const inputImage = await faceapi.fetchImage(tempImageUrl);

  showLoading(true, 25, "Đang phát hiện khuôn mặt...");
  const inputDetections = await faceapi
    .detectAllFaces(inputImage)
    .withFaceLandmarks()
    .withFaceDescriptors();

  if (inputDetections.length === 0) {
    throw new Error("Không tìm thấy khuôn mặt trong ảnh");
  }

  // Tạo bộ mô tả khuôn mặt từ các bài viết
  const labeledDescriptors = [];
  const totalPosts = posts.length;
  let processedPosts = 0;

  showLoading(true, 30, "Đang tải dữ liệu bài viết...");

  for (const post of posts) {
    // Cập nhật tiến trình
    const progress = 30 + Math.floor((processedPosts / totalPosts) * 50);
    showLoading(
      true,
      progress,
      `Đang xử lý bài viết ${processedPosts + 1}/${totalPosts}...`
    );

    const descriptors = [];
    for (const img of post.Images) {
      try {
        const image = await faceapi.fetchImage(img.Url);
        const detection = await faceapi
          .detectSingleFace(image)
          .withFaceLandmarks()
          .withFaceDescriptor();

        if (detection) {
          descriptors.push(detection.descriptor);
        }
      } catch (e) {
        console.error(`Lỗi khi xử lý ảnh ${img.Url}:`, e);
      }
    }

    if (descriptors.length > 0) {
      labeledDescriptors.push(
        new faceapi.LabeledFaceDescriptors(
          `post_${post.Id}_${post.HoTen.replace(/\s+/g, "_")}`,
          descriptors
        )
      );
    }

    processedPosts++;
  }

  showLoading(true, 85, "Đang so sánh khuôn mặt...");

  if (labeledDescriptors.length === 0) {
    throw new Error("Không có dữ liệu khuôn mặt để so sánh");
  }

  const faceMatcher = new faceapi.FaceMatcher(labeledDescriptors, 0.5);

  // So khớp kết quả
  const results = inputDetections.map((d) => {
    const bestMatch = faceMatcher.findBestMatch(d.descriptor);
    return {
      postId: parseInt(bestMatch.label.split("_")[1]),
      hoTen: bestMatch.label.split("_").slice(2).join(" "),
      distance: bestMatch.distance,
      similarity: (1 - bestMatch.distance) * 100,
    };
  });

  // Nhóm kết quả theo bài viết
  const postResults = {};
  results.forEach((result) => {
    if (!postResults[result.postId]) {
      const post = posts.find((p) => p.Id === result.postId);
      postResults[result.postId] = {
        post: post,
        similarities: [],
      };
    }
    postResults[result.postId].similarities.push(result.similarity);
  });

  // Tính điểm trung bình và sắp xếp
  return Object.values(postResults)
    .map((item) => {
      const avgSimilarity =
        item.similarities.reduce((a, b) => a + b, 0) / item.similarities.length;
      return {
        ...item.post,
        similarity: avgSimilarity.toFixed(2),
        matchCount: item.similarities.length,
      };
    })
    .sort((a, b) => b.similarity - a.similarity);
}

// Hiển thị kết quả
function displayResults(results) {
  const resultsContainer = document.getElementById("searchResults");
  resultsContainer.innerHTML = "";

  if (!results || results.length === 0) {
    resultsContainer.innerHTML = `
            <div class="alert alert-info">
                Không tìm thấy bài viết nào phù hợp
                <button class="btn btn-sm btn-outline-primary ms-2" onclick="document.getElementById('searchImage').click()">
                    Thử với ảnh khác
                </button>
            </div>
        `;
    return;
  }

  // Hiển thị kết quả tốt nhất đầu tiên
  const bestMatch = results[0];
  resultsContainer.innerHTML += `
        <div class="card best-match mb-4">
            <div class="card-header bg-primary text-white">
                <h5 class="mb-0">Kết quả phù hợp nhất</h5>
            </div>
            <div class="card-body">
                <div class="row">
                    <div class="col-md-3">
                        ${
                          bestMatch.Images && bestMatch.Images[0]
                            ? `<img src="${bestMatch.Images[0].Url}" class="img-thumbnail" alt="Ảnh đại diện">`
                            : '<div class="no-image">Không có ảnh</div>'
                        }
                    </div>
                    <div class="col-md-9">
                        <h3>${bestMatch.HoTen} <span class="badge bg-success">${
    bestMatch.similarity
  }% phù hợp</span></h3>
                        <p><strong>Tiêu đề:</strong> ${
                          bestMatch.TieuDe || "Không có"
                        }</p>
                        <p><strong>Đặc điểm:</strong> ${
                          bestMatch.DacDiem || "Không có"
                        }</p>
                        <p><strong>Khu vực:</strong> ${
                          bestMatch.KhuVuc || "Không rõ"
                        }</p>
                        <a href="/TimNguoi/ChiTiet/${
                          bestMatch.Id
                        }" class="btn btn-primary mt-2">
                            Xem chi tiết
                        </a>
                    </div>
                </div>
            </div>
        </div>
    `;

  // Hiển thị các kết quả khác
  if (results.length > 1) {
    const otherResults = document.createElement("div");
    otherResults.className = "other-results";
    otherResults.innerHTML = `<h4 class="mb-3">Các kết quả khác</h4>`;

    const row = document.createElement("div");
    row.className = "row row-cols-1 row-cols-md-2 g-4";

    results.slice(1).forEach((result) => {
      const col = document.createElement("div");
      col.className = "col";
      col.innerHTML = `
                <div class="card h-100">
                    <div class="card-body">
                        <div class="d-flex">
                            <div class="flex-shrink-0 me-3">
                                ${
                                  result.Images && result.Images[0]
                                    ? `<img src="${result.Images[0].Url}" class="rounded" width="80" height="80" alt="Ảnh đại diện">`
                                    : '<div class="no-image small">Không có ảnh</div>'
                                }
                            </div>
                            <div>
                                <h5 class="card-title">${
                                  result.HoTen
                                } <span class="badge bg-info">${
        result.similarity
      }%</span></h5>
                                <p class="card-text text-muted small">${
                                  result.TieuDe || "Không có tiêu đề"
                                }</p>
                                <a href="/TimNguoi/ChiTiet/${
                                  result.Id
                                }" class="btn btn-sm btn-outline-primary">Xem chi tiết</a>
                            </div>
                        </div>
                    </div>
                </div>
            `;
      row.appendChild(col);
    });

    otherResults.appendChild(row);
    resultsContainer.appendChild(otherResults);
  }

  // Thêm nút tìm kiếm lại
  resultsContainer.innerHTML += `
        <div class="text-center mt-4">
            <button class="btn btn-outline-primary" onclick="document.getElementById('searchImage').click()">
                <i class="fas fa-search"></i> Tìm kiếm với ảnh khác
            </button>
        </div>
    `;
}

// Khởi tạo khi trang được tải
document.addEventListener("DOMContentLoaded", () => {
  initFaceRecognition();

  const searchInput = document.getElementById("searchImage");
  if (searchInput) {
    searchInput.addEventListener("change", handleImageUpload);
  }

  // Thêm nút hủy
  document.body.insertAdjacentHTML(
    "beforeend",
    `
        <button id="cancelButton" class="btn btn-danger d-none" onclick="cancelSearch()">
            <i class="fas fa-times-circle"></i> Hủy tìm kiếm
        </button>
    `
  );
});

// So sánh ảnh chụp từ camera và ảnh CCCD để xác thực CCCD

$(document).ready(function () {
  if (window.initFaceRecognition) {
    initFaceRecognition();
  }
});
// Hàm xác thực nhận vào Image object thay vì file
async function verifyCCCDWithImages(cameraImg, cccdImg) {
    const progressDiv = document.getElementById("cccdVerifyProgress");
    const resultDiv = document.getElementById("cccdVerifyResult");
    try {
        progressDiv.innerText = "60% - Đang phát hiện khuôn mặt...";

        const options = new faceapi.TinyFaceDetectorOptions({
            inputSize: 320,
            scoreThreshold: 0.5,
        });
        const camDetections = await faceapi
            .detectAllFaces(cameraImg, options)
            .withFaceLandmarks()
            .withFaceDescriptors();
        const cccdDetections = await faceapi
            .detectAllFaces(cccdImg, options)
            .withFaceLandmarks()
            .withFaceDescriptors();

        if (camDetections.length === 0 || cccdDetections.length === 0) {
            progressDiv.innerText = "";
            resultDiv.innerHTML =
                '<div class="alert alert-danger">Không phát hiện được khuôn mặt trên một trong hai ảnh!</div>';
            return false;
        }

        progressDiv.innerText = "80% - Đang so sánh khuôn mặt...";

        let similarities = [];
        for (let cam of camDetections) {
            for (let cccd of cccdDetections) {
                const dist = faceapi.euclideanDistance(cam.descriptor, cccd.descriptor);
                similarities.push((1 - dist) * 100);
            }
        }
        const similarity = (
            similarities.reduce((a, b) => a + b, 0) / similarities.length
        ).toFixed(2);
        progressDiv.innerText = "100% - Hoàn thành";
        const isVerified = similarity > 50;

        resultDiv.innerHTML = isVerified
            ? `<div class="alert alert-success">✅ Xác thực thành công! Độ tương đồng: <b>${similarity}%</b></div>`
            : `<div class="alert alert-danger">❌ Xác thực thất bại! Độ tương đồng: <b>${similarity}%</b></div>`;

        return isVerified;
    } catch (err) {
        progressDiv.innerText = "";
        resultDiv.innerHTML =
            '<div class="alert alert-danger">Lỗi xác thực CCCD: ' +
            err.message +
            "</div>";
        return false;
    }
}
// Phân tích và vẽ khung khuôn mặt khi chọn ảnh
async function analyzeAndDrawFace(inputId, imgId, canvasId) {
  const fileInput = document.getElementById(inputId);
  const imgTag = document.getElementById(imgId);
  const canvas = document.getElementById(canvasId);

  if (!fileInput.files[0]) {
    imgTag.classList.add("d-none");
    canvas.getContext("2d").clearRect(0, 0, canvas.width, canvas.height);
    return;
  }

  // Đọc file thành ảnh
  const file = fileInput.files[0];
  const reader = new FileReader();
  reader.onload = async function (e) {
    imgTag.src = e.target.result;
    imgTag.classList.remove("d-none");
    imgTag.onload = async function () {
      // Resize canvas theo ảnh
      canvas.width = imgTag.width;
      canvas.height = imgTag.height;
      const ctx = canvas.getContext("2d");
      ctx.clearRect(0, 0, canvas.width, canvas.height);

      // Phát hiện khuôn mặt
      if (window.faceapi) {
        const options = new faceapi.TinyFaceDetectorOptions({
          inputSize: 320,
          scoreThreshold: 0.5,
        });
        const detections = await faceapi
          .detectAllFaces(imgTag, options)
          .withFaceLandmarks()
          .withFaceDescriptors();
        if (detections.length > 0) {
          // Vẽ khung mặt
          const displaySize = { width: imgTag.width, height: imgTag.height };
          faceapi.matchDimensions(canvas, displaySize);
          const resized = faceapi.resizeResults(detections, displaySize);
          faceapi.draw.drawDetections(canvas, resized);
          faceapi.draw.drawFaceLandmarks(canvas, resized);
        }
      }
    };
  };
  reader.readAsDataURL(file);
}

function fileToImage(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = (e) => {
      const img = new Image();
      img.onload = () => resolve(img);
      img.onerror = reject;
      img.src = e.target.result;
    };
    reader.onerror = reject;
    reader.readAsDataURL(file);
  });
}
function previewImage(input, imgId) {
  const file = input.files[0];
  const img = document.getElementById(imgId);
  if (file) {
    const reader = new FileReader();
    reader.onload = (e) => {
      img.src = e.target.result;
      img.classList.remove("d-none");
    };
    reader.readAsDataURL(file);
  } else {
    img.src = "#";
    img.classList.add("d-none");
  }
}
function startCCCDVerification() {
  const cccdFile = document.getElementById("cccdImage").files[0];
  const resultDiv = document.getElementById("cccdVerifyResult");
  resultDiv.innerHTML = "";

  // Lấy ảnh từ canvas (ảnh vừa chụp)
  const preview = document.getElementById("preview");
  if (!cccdFile || !preview.src || preview.style.display === "none") {
    resultDiv.innerHTML =
      '<div class="alert alert-warning">Vui lòng chọn đủ hai ảnh!</div>';
    return;
  }

  // Chuyển ảnh preview (base64) thành Image object
  const cameraImg = new Image();
  cameraImg.src = preview.src;
  cameraImg.onload = async function () {
    // Đọc file CCCD thành Image object
    fileToImage(cccdFile).then((cccdImg) => {
      verifyCCCDWithImages(cameraImg, cccdImg);
    });
  };
}
